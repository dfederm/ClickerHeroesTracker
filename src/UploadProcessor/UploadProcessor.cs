// <copyright file="UploadProcessor.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.UploadProcessing
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using ClickerHeroesTrackerWebsite.Instrumentation;
    using ClickerHeroesTrackerWebsite.Models;
    using ClickerHeroesTrackerWebsite.Models.Game;
    using ClickerHeroesTrackerWebsite.Models.SaveData;
    using ClickerHeroesTrackerWebsite.Models.Settings;
    using ClickerHeroesTrackerWebsite.Models.Stats;
    using ClickerHeroesTrackerWebsite.Services.Database;
    using ClickerHeroesTrackerWebsite.Services.UploadProcessing;
    using Microsoft.ApplicationInsights;
    using Microsoft.Extensions.Options;
    using Microsoft.WindowsAzure.Storage.Queue;
    using Newtonsoft.Json;

    internal sealed class UploadProcessor
    {
        private readonly IOptions<DatabaseSettings> databaseSettingsOptions;
        private readonly GameData gameData;
        private readonly TelemetryClient telemetryClient;

        private readonly Dictionary<UploadProcessingMessagePriority, CloudQueue> queues;

        /// <summary>
        /// Initializes a new instance of the <see cref="UploadProcessor"/> class.
        /// </summary>
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
                        await Task.Delay(10000);
                    }
                    else
                    {
                        var processed = this.ProcessMessage(message);
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
            var queue = queueClient.GetQueueReference($"upload-processing-{priority.ToString().ToLower()}-priority");
            queue.CreateIfNotExistsAsync().Wait();
            return queue;
        }

        private bool ProcessMessage(CloudQueueMessage queueMessage)
        {
            var properties = new Dictionary<string, string>();
            properties.Add("CloudQueueMessage-DequeueCount", queueMessage.DequeueCount.ToString());
            properties.Add("CloudQueueMessage-InsertionTime", queueMessage.InsertionTime.ToString());
            properties.Add("CloudQueueMessage-Id", queueMessage.Id);

            this.telemetryClient.TrackEvent("UploadProcessor-Recieved", properties);

            using (var counterProvider = new CounterProvider(this.telemetryClient))
            using (var databaseCommandFactory = new DatabaseCommandFactory(
                this.databaseSettingsOptions,
                counterProvider))
            using (counterProvider.Measure(Counter.ProcessUpload))
            {
                int uploadId = -1;
                var userSettingsProvider = new UserSettingsProvider(databaseCommandFactory);
                try
                {
                    var message = JsonConvert.DeserializeObject<UploadProcessingMessage>(queueMessage.AsString);
                    if (message == null)
                    {
                        this.telemetryClient.TrackEvent("UploadProcessor-Abandoned-ParseMessage", properties);
                        return false;
                    }

                    uploadId = message.UploadId;
                    properties.Add("UploadId", uploadId.ToString());

                    this.CurrentUploadId = uploadId;

                    string uploadContent;
                    string userId;
                    PlayStyle playStyle;
                    this.GetUploadDetails(databaseCommandFactory, uploadId, out uploadContent, out userId, out playStyle);
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
                        return true;
                    }

                    var userSettings = userSettingsProvider.Get(userId);
                    if (userSettings == null)
                    {
                        this.telemetryClient.TrackEvent("UploadProcessor-Abandoned-FetchUserSettings", properties);
                        return false;
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
                        savedGame,
                        this.telemetryClient);
                    var outsiderLevels = new OutsiderLevelsModel(
                        savedGame,
                        this.telemetryClient);
                    var miscellaneousStatsModel = new MiscellaneousStatsModel(
                        gameData,
                        savedGame);

                    /* Build a query that looks like this:
                        MERGE INTO AncientLevels WITH (HOLDLOCK)
                        USING
                            (VALUES (123, 1, 100), (123, 2, 100), ... )
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
                            ancientLevelsCommandText.Append(pair.Key.Id);
                            ancientLevelsCommandText.Append(",");
                            ancientLevelsCommandText.Append(pair.Value.AncientLevel);
                            ancientLevelsCommandText.Append(")");

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
                            outsiderLevelsCommandText.Append(pair.Value.Level);
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
                                    @AscensionsLifetime,
                                    @MaxTranscendentPrimalReward,
                                    @BossLevelToTranscendentPrimalCap) )
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
                                    AscensionsLifetime,
                                    MaxTranscendentPrimalReward,
                                    BossLevelToTranscendentPrimalCap)
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
                                AscensionsLifetime = Input.AscensionsLifetime,
                                MaxTranscendentPrimalReward = Input.MaxTranscendentPrimalReward,
                                BossLevelToTranscendentPrimalCap = Input.BossLevelToTranscendentPrimalCap
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
                                AscensionsLifetime,
                                MaxTranscendentPrimalReward,
                                BossLevelToTranscendentPrimalCap)
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
                                Input.AscensionsLifetime,
                                Input.MaxTranscendentPrimalReward,
                                Input.BossLevelToTranscendentPrimalCap);";
                    var computedStatsCommandParameters = new Dictionary<string, object>
                    {
                        { "@UploadId", uploadId },
                        { "@TitanDamage", miscellaneousStatsModel.TitanDamage },
                        { "@SoulsSpent", miscellaneousStatsModel.HeroSoulsSpent },
                        { "@HeroSoulsSacrificed", miscellaneousStatsModel.HeroSoulsSacrificed },
                        { "@TotalAncientSouls", miscellaneousStatsModel.TotalAncientSouls },
                        { "@TranscendentPower", miscellaneousStatsModel.TranscendentPower },
                        { "@Rubies", miscellaneousStatsModel.Rubies },
                        { "@HighestZoneThisTranscension", miscellaneousStatsModel.HighestZoneThisTranscension },
                        { "@HighestZoneLifetime", miscellaneousStatsModel.HighestZoneLifetime },
                        { "@AscensionsThisTranscension", miscellaneousStatsModel.AscensionsThisTranscension },
                        { "@AscensionsLifetime", miscellaneousStatsModel.AscensionsLifetime },
                        { "@MaxTranscendentPrimalReward", miscellaneousStatsModel.MaxTranscendentPrimalReward },
                        { "@BossLevelToTranscendentPrimalCap", miscellaneousStatsModel.BossLevelToTranscendentPrimalCap },
                    };

                    using (var command = databaseCommandFactory.Create())
                    {
                        command.BeginTransaction();

                        if (ancientLevelsCommandText.Length > 0)
                        {
                            command.CommandText = ancientLevelsCommandText.ToString();
                            command.ExecuteNonQuery();
                        }

                        if (outsiderLevelsCommandText.Length > 0)
                        {
                            command.CommandText = outsiderLevelsCommandText.ToString();
                            command.ExecuteNonQuery();
                        }

                        command.CommandText = ComputedStatsCommandText;
                        command.Parameters = computedStatsCommandParameters;
                        command.ExecuteNonQuery();

                        var commited = command.CommitTransaction();
                        if (!commited)
                        {
                            this.telemetryClient.TrackEvent("UploadProcessor-Abandoned-CommitTransaction", properties);
                            return false;
                        }
                    }

                    this.telemetryClient.TrackEvent("UploadProcessor-Complete", properties);
                    return true;
                }
                catch (Exception e)
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

        private void GetUploadDetails(
            IDatabaseCommandFactory databaseCommandFactory,
            int uploadId,
            out string uploadContent,
            out string userId,
            out PlayStyle playStyle)
        {
            const string CommandText = @"
	            SELECT UploadContent, UserId, PlayStyle
	            FROM Uploads
	            WHERE Id = @UploadId";
            var commandParameters = new Dictionary<string, object>
            {
                { "@UploadId", uploadId },
            };
            using (var command = databaseCommandFactory.Create(
                CommandText,
                commandParameters))
            using (var reader = command.ExecuteReader())
            {
                reader.Read();
                uploadContent = reader["UploadContent"].ToString();
                userId = reader["UserId"].ToString();

                if (!Enum.TryParse(reader["PlayStyle"].ToString(), out playStyle))
                {
                    playStyle = default(PlayStyle);
                }
            }
        }
    }
}