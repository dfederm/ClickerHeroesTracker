// <copyright file="UploadProcessor.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTracker.UploadProcessor
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using ClickerHeroesTrackerWebsite.Models.Game;
    using ClickerHeroesTrackerWebsite.Models.SaveData;
    using ClickerHeroesTrackerWebsite.Models.Stats;
    using ClickerHeroesTrackerWebsite.Services.Database;
    using ClickerHeroesTrackerWebsite.Services.UploadProcessing;
    using ClickerHeroesTrackerWebsite.Utility;
    using Microsoft.ApplicationInsights;
    using Microsoft.Azure.Storage.Queue;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;

    internal sealed class UploadProcessor
    {
        private readonly IOptions<DatabaseSettings> databaseSettingsOptions;
        private readonly GameData gameData;
        private readonly TelemetryClient telemetryClient;

        private readonly Dictionary<UploadProcessingMessagePriority, CloudQueue> queues;

        public UploadProcessor(
            IOptions<DatabaseSettings> databaseSettingsOptions,
            GameData gameData,
            TelemetryClient telemetryClient,
            CloudQueueClient queueClient)
        {
            this.databaseSettingsOptions = databaseSettingsOptions;
            this.gameData = gameData;
            this.telemetryClient = telemetryClient;
            this.queues = new Dictionary<UploadProcessingMessagePriority, CloudQueue>
            {
                { UploadProcessingMessagePriority.Low, GetQueue(queueClient, UploadProcessingMessagePriority.Low) },
                { UploadProcessingMessagePriority.High, GetQueue(queueClient, UploadProcessingMessagePriority.High) },
            };
        }

        public int? CurrentUploadId { get; private set; }

        public async Task ProcessAsync(CancellationToken cancellationToken)
        {
            const int MaxProcessAttempts = 10;
            var visibilityTimeout = TimeSpan.FromMinutes(10);

            while (!cancellationToken.IsCancellationRequested)
            {
                foreach (var queue in this.queues.Values)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    var message = await queue.GetMessageAsync(visibilityTimeout, null, null, cancellationToken);
                    if (message == null)
                    {
                        // if there was no work to do, wait for a bit to avoid spamming the empty queue with requests
                        await Task.Delay(10000, cancellationToken);
                    }
                    else
                    {
                        var processed = await this.ProcessMessageAsync(message);
                        if (processed || message.DequeueCount > MaxProcessAttempts)
                        {
                            await queue.DeleteMessageAsync(message);
                        }
                    }
                }
            }
        }

        private static CloudQueue GetQueue(CloudQueueClient queueClient, UploadProcessingMessagePriority priority)
        {
            var queue = queueClient.GetQueueReference($"upload-processing-{priority.ToString().ToLower(CultureInfo.InvariantCulture)}-priority");
            queue.CreateIfNotExistsAsync().Wait();
            return queue;
        }

        private static async Task<(string UploadContent, string UserId)> GetUploadDetailsAsync(
            IDatabaseCommandFactory databaseCommandFactory,
            int uploadId)
        {
            const string CommandText = @"
	            SELECT UploadContent, UserId
	            FROM Uploads
	            WHERE Id = @UploadId";
            var commandParameters = new Dictionary<string, object>
            {
                { "@UploadId", uploadId },
            };
            using (var command = databaseCommandFactory.Create(
                CommandText,
                commandParameters))
            using (var reader = await command.ExecuteReaderAsync())
            {
                reader.Read();

                var uploadContent = reader["UploadContent"].ToString();
                var userId = reader["UserId"].ToString();

                return (uploadContent, userId);
            }
        }

        private async Task<bool> ProcessMessageAsync(CloudQueueMessage queueMessage)
        {
            var properties = new Dictionary<string, string>
            {
                { "CloudQueueMessage-DequeueCount", queueMessage.DequeueCount.ToString() },
                { "CloudQueueMessage-InsertionTime", queueMessage.InsertionTime.ToString() },
                { "CloudQueueMessage-Id", queueMessage.Id },
            };

            this.telemetryClient.TrackEvent("UploadProcessor-Recieved", properties);

            var databaseCommandFactory = new DatabaseCommandFactory(this.databaseSettingsOptions);
            try
            {
                var message = JsonConvert.DeserializeObject<UploadProcessingMessage>(queueMessage.AsString);
                if (message == null)
                {
                    this.telemetryClient.TrackEvent("UploadProcessor-Abandoned-ParseMessage", properties);
                    return false;
                }

                var uploadId = message.UploadId;
                properties.Add("UploadId", uploadId.ToString());

                this.CurrentUploadId = uploadId;

                var (uploadContent, userId) = await GetUploadDetailsAsync(databaseCommandFactory, uploadId);
                properties.Add("UserId", userId);
                if (string.IsNullOrWhiteSpace(uploadContent))
                {
                    this.telemetryClient.TrackEvent("UploadProcessor-Abandoned-FetchUpload", properties);
                    return false;
                }

                // Handle legacy uplaods where the upload content was missing.
                if (uploadContent.Equals("LEGACY", StringComparison.OrdinalIgnoreCase))
                {
                    this.telemetryClient.TrackEvent("UploadProcessor-Complete-Legacy", properties);
                    using (var command = databaseCommandFactory.Create())
                    {
                        command.CommandText = "UPDATE Uploads SET LastComputeTime = getutcdate() WHERE Id = " + uploadId;
                        await command.ExecuteNonQueryAsync();
                    }

                    return true;
                }

                var savedGame = SavedGame.Parse(uploadContent);
                if (savedGame == null)
                {
                    this.telemetryClient.TrackEvent("UploadProcessor-Abandoned-ParseUpload", properties);
                    return false;
                }

                this.telemetryClient.TrackEvent("UploadProcessor-Processing", properties);
                var ancientLevels = new AncientLevelsModel(
                    this.gameData,
                    savedGame);
                var outsiderLevels = new OutsiderLevelsModel(
                    this.gameData,
                    savedGame);
                var computedStats = new ComputedStats(savedGame);

                /* Build a query that looks like this:
                    MERGE INTO AncientLevels WITH (HOLDLOCK)
                    USING
                        (VALUES (123, 1, '100'), (123, 2, '100'), ... )
                            AS Input(UploadId, AncientId, Level)
                        ON AncientLevels.UploadId = Input.UploadId
                        AND AncientLevels.AncientId = Input.AncientId
                    WHEN MATCHED THEN
                        UPDATE
                        SET
                            Level = Input.Level
                    WHEN NOT MATCHED THEN
                        INSERT (UploadId, AncientId, Level)
                        VALUES (Input.UploadId, Input.AncientId, Input.Level);
                */
                var ancientLevelsCommandText = new StringBuilder();
                if (ancientLevels.AncientLevels.Count > 0)
                {
                    ancientLevelsCommandText.Append(@"
                    MERGE INTO AncientLevels WITH (HOLDLOCK)
                    USING
                        ( VALUES ");
                    var isFirst = true;
                    foreach (var pair in ancientLevels.AncientLevels)
                    {
                        if (!isFirst)
                        {
                            ancientLevelsCommandText.Append(",");
                        }

                        // No need to sanitize, these are all just numbers
                        ancientLevelsCommandText.Append("(");
                        ancientLevelsCommandText.Append(uploadId);
                        ancientLevelsCommandText.Append(",");
                        ancientLevelsCommandText.Append(pair.Key);
                        ancientLevelsCommandText.Append(",'");
                        ancientLevelsCommandText.Append(pair.Value.ToTransportableString());
                        ancientLevelsCommandText.Append("')");

                        isFirst = false;
                    }

                    ancientLevelsCommandText.Append(@"
                        )
                            AS Input(UploadId, AncientId, Level)
                        ON AncientLevels.UploadId = Input.UploadId
                        AND AncientLevels.AncientId = Input.AncientId
                    WHEN MATCHED THEN
                        UPDATE
                        SET
                            Level = Input.Level
                    WHEN NOT MATCHED THEN
                        INSERT (UploadId, AncientId, Level)
                        VALUES (Input.UploadId, Input.AncientId, Input.Level);");
                }

                /* Build a query that looks like this:
                    MERGE INTO OutsiderLevels WITH (HOLDLOCK)
                    USING
                        (VALUES (123, 1, 100), (123, 2, 100), ... )
                            AS Input(UploadId, OutsiderId, Level)
                        ON OutsiderLevels.UploadId = Input.UploadId
                        AND OutsiderLevels.OutsiderId = Input.OutsiderId
                    WHEN MATCHED THEN
                        UPDATE
                        SET
                            Level = Input.Level
                    WHEN NOT MATCHED THEN
                        INSERT (UploadId, OutsiderId, Level)
                        VALUES (Input.UploadId, Input.OutsiderId, Input.Level);
                */
                var outsiderLevelsCommandText = new StringBuilder();
                if (outsiderLevels.OutsiderLevels.Count > 0)
                {
                    outsiderLevelsCommandText.Append(@"
                    MERGE INTO OutsiderLevels WITH (HOLDLOCK)
                    USING
                        ( VALUES ");
                    var isFirst = true;
                    foreach (var pair in outsiderLevels.OutsiderLevels)
                    {
                        if (!isFirst)
                        {
                            outsiderLevelsCommandText.Append(",");
                        }

                        // No need to sanitize, these are all just numbers
                        outsiderLevelsCommandText.Append("(");
                        outsiderLevelsCommandText.Append(uploadId);
                        outsiderLevelsCommandText.Append(",");
                        outsiderLevelsCommandText.Append(pair.Key);
                        outsiderLevelsCommandText.Append(",");
                        outsiderLevelsCommandText.Append(pair.Value);
                        outsiderLevelsCommandText.Append(")");

                        isFirst = false;
                    }

                    outsiderLevelsCommandText.Append(@"
                        )
                            AS Input(UploadId, OutsiderId, Level)
                        ON OutsiderLevels.UploadId = Input.UploadId
                        AND OutsiderLevels.OutsiderId = Input.OutsiderId
                    WHEN MATCHED THEN
                        UPDATE
                        SET
                            Level = Input.Level
                    WHEN NOT MATCHED THEN
                        INSERT (UploadId, OutsiderId, Level)
                        VALUES (Input.UploadId, Input.OutsiderId, Input.Level);");
                }

                const string ComputedStatsCommandText = @"
                    MERGE INTO ComputedStats WITH (HOLDLOCK)
                    USING
                        (VALUES (
                                @UploadId,
                                @TitanDamage,
                                @SoulsSpent,
                                @HeroSoulsSacrificed,
                                @TotalAncientSouls,
                                @TranscendentPower,
                                @Rubies,
                                @HighestZoneThisTranscension,
                                @HighestZoneLifetime,
                                @AscensionsThisTranscension,
                                @AscensionsLifetime) )
                            AS Input(
                                UploadId,
                                TitanDamage,
                                SoulsSpent,
                                HeroSoulsSacrificed,
                                TotalAncientSouls,
                                TranscendentPower,
                                Rubies,
                                HighestZoneThisTranscension,
                                HighestZoneLifetime,
                                AscensionsThisTranscension,
                                AscensionsLifetime)
                        ON ComputedStats.UploadId = Input.UploadId
                    WHEN MATCHED THEN
                        UPDATE
                        SET
                            TitanDamage = Input.TitanDamage,
                            SoulsSpent = Input.SoulsSpent,
                            HeroSoulsSacrificed = Input.HeroSoulsSacrificed,
                            TotalAncientSouls = Input.TotalAncientSouls,
                            TranscendentPower = Input.TranscendentPower,
                            Rubies = Input.Rubies,
                            HighestZoneThisTranscension = Input.HighestZoneThisTranscension,
                            HighestZoneLifetime = Input.HighestZoneLifetime,
                            AscensionsThisTranscension = Input.AscensionsThisTranscension,
                            AscensionsLifetime = Input.AscensionsLifetime
                    WHEN NOT MATCHED THEN
                        INSERT (
                            UploadId,
                            TitanDamage,
                            SoulsSpent,
                            HeroSoulsSacrificed,
                            TotalAncientSouls,
                            TranscendentPower,
                            Rubies,
                            HighestZoneThisTranscension,
                            HighestZoneLifetime,
                            AscensionsThisTranscension,
                            AscensionsLifetime)
                        VALUES (
                            Input.UploadId,
                            Input.TitanDamage,
                            Input.SoulsSpent,
                            Input.HeroSoulsSacrificed,
                            Input.TotalAncientSouls,
                            Input.TranscendentPower,
                            Input.Rubies,
                            Input.HighestZoneThisTranscension,
                            Input.HighestZoneLifetime,
                            Input.AscensionsThisTranscension,
                            Input.AscensionsLifetime);";
                var computedStatsCommandParameters = new Dictionary<string, object>
                {
                    { "@UploadId", uploadId },
                    { "@TitanDamage", computedStats.TitanDamage },
                    { "@SoulsSpent", computedStats.HeroSoulsSpent },
                    { "@HeroSoulsSacrificed", computedStats.HeroSoulsSacrificed },
                    { "@TotalAncientSouls", computedStats.TotalAncientSouls },
                    { "@TranscendentPower", computedStats.TranscendentPower },
                    { "@Rubies", computedStats.Rubies },
                    { "@HighestZoneThisTranscension", computedStats.HighestZoneThisTranscension },
                    { "@HighestZoneLifetime", computedStats.HighestZoneLifetime },
                    { "@AscensionsThisTranscension", computedStats.AscensionsThisTranscension },
                    { "@AscensionsLifetime", computedStats.AscensionsLifetime },
                };

                using (var command = databaseCommandFactory.Create())
                {
                    await command.BeginTransactionAsync();

                    if (ancientLevelsCommandText.Length > 0)
                    {
                        command.CommandText = ancientLevelsCommandText.ToString();
                        await command.ExecuteNonQueryAsync();
                    }

                    if (outsiderLevelsCommandText.Length > 0)
                    {
                        command.CommandText = outsiderLevelsCommandText.ToString();
                        await command.ExecuteNonQueryAsync();
                    }

                    command.CommandText = ComputedStatsCommandText;
                    command.Parameters = computedStatsCommandParameters;
                    await command.ExecuteNonQueryAsync();

                    command.CommandText = "UPDATE Uploads SET LastComputeTime = getutcdate() WHERE Id = " + uploadId;
                    await command.ExecuteNonQueryAsync();

                    try
                    {
                        command.CommitTransaction();
                    }
#pragma warning disable CA1031 // Do not catch general exception types
                    catch (Exception)
#pragma warning restore CA1031 // Do not catch general exception types
                    {
                        this.telemetryClient.TrackEvent("UploadProcessor-Abandoned-CommitTransaction", properties);
                        return false;
                    }
                }

                this.telemetryClient.TrackEvent("UploadProcessor-Complete", properties);
                return true;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                this.telemetryClient.TrackException(e, properties);
                return false;
            }
            finally
            {
                this.CurrentUploadId = null;
            }
        }
    }
}