// <copyright file="ProgressData.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Api.Users
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ClickerHeroesTrackerWebsite.Models.Game;
    using ClickerHeroesTrackerWebsite.Services.Database;
    using ClickerHeroesTrackerWebsite.Utility;
    using Microsoft.ApplicationInsights;
    using Newtonsoft.Json;

    /// <summary>
    /// An aggregation of progress data for a user.
    /// </summary>
    public class ProgressData
    {
        public IDictionary<DateTime, string> TitanDamageData { get; } = new SortedDictionary<DateTime, string>();

        public IDictionary<DateTime, string> SoulsSpentData { get; } = new SortedDictionary<DateTime, string>();

        public IDictionary<DateTime, string> HeroSoulsSacrificedData { get; } = new SortedDictionary<DateTime, string>();

        public IDictionary<DateTime, string> TotalAncientSoulsData { get; } = new SortedDictionary<DateTime, string>();

        public IDictionary<DateTime, string> TranscendentPowerData { get; } = new SortedDictionary<DateTime, string>();

        public IDictionary<DateTime, string> RubiesData { get; } = new SortedDictionary<DateTime, string>();

        public IDictionary<DateTime, string> HighestZoneThisTranscensionData { get; } = new SortedDictionary<DateTime, string>();

        public IDictionary<DateTime, string> HighestZoneLifetimeData { get; } = new SortedDictionary<DateTime, string>();

        public IDictionary<DateTime, string> AscensionsThisTranscensionData { get; } = new SortedDictionary<DateTime, string>();

        public IDictionary<DateTime, string> AscensionsLifetimeData { get; } = new SortedDictionary<DateTime, string>();

        public IDictionary<string, IDictionary<DateTime, string>> AncientLevelData { get; } = new SortedDictionary<string, IDictionary<DateTime, string>>(StringComparer.OrdinalIgnoreCase);

        public IDictionary<string, IDictionary<DateTime, string>> OutsiderLevelData { get; } = new SortedDictionary<string, IDictionary<DateTime, string>>(StringComparer.OrdinalIgnoreCase);

        public static async Task<ProgressData> CreateAsync(
            GameData gameData,
            TelemetryClient telemetryClient,
            IDatabaseCommandFactory databaseCommandFactory,
            string userId,
            DateTime startTime,
            DateTime endTime)
        {
            var progressData = new ProgressData();

            // SQL's datetime2 has no timezone so we need to explicitly convert to UTC
            startTime = startTime.ToUniversalTime();
            endTime = endTime.ToUniversalTime();

            var parameters = new Dictionary<string, object>
            {
                { "@UserId", userId },
                { "@StartTime", startTime },
                { "@EndTime", endTime },
            };

            const string GetProgressDataCommandText = @"
                -- Create a temp table that scopes the Uploads
                CREATE TABLE #ScopedUploads
                (
                    Id  INT NOT NULL, 
                    UploadTime  DATETIME2(0) NOT NULL, 
                );

                -- Populate temp table
                INSERT INTO #ScopedUploads (Id, UploadTime)
                SELECT Id, UploadTime
                FROM Uploads
                WHERE UserId = @UserId
                AND UploadTime >= ISNULL(@StartTime, '0001-01-01 00:00:00')
                AND UploadTime <= ISNULL(@EndTime, '9999-12-31 23:59:59');

                -- Computed Stats
                SELECT #ScopedUploads.UploadTime,
                       RTRIM(ComputedStats.TitanDamage) AS TitanDamage,
                       RTRIM(ComputedStats.SoulsSpent) AS SoulsSpent,
                       RTRIM(ComputedStats.HeroSoulsSacrificed) AS HeroSoulsSacrificed,
                       ComputedStats.TotalAncientSouls,
                       ComputedStats.TranscendentPower,
                       ComputedStats.Rubies,
                       ComputedStats.HighestZoneThisTranscension,
                       ComputedStats.HighestZoneLifetime,
                       ComputedStats.AscensionsThisTranscension,
                       ComputedStats.AscensionsLifetime
                FROM ComputedStats
                INNER JOIN #ScopedUploads
                ON ComputedStats.UploadId = #ScopedUploads.Id;

                -- Ancient Levels
                SELECT #ScopedUploads.UploadTime, AncientLevels.AncientId, RTRIM(AncientLevels.Level) AS Level
                FROM AncientLevels
                INNER JOIN #ScopedUploads
                ON AncientLevels.UploadId = #ScopedUploads.Id;

                -- Outsider Levels
                SELECT #ScopedUploads.UploadTime, OutsiderLevels.OutsiderId, OutsiderLevels.Level
                FROM OutsiderLevels
                INNER JOIN #ScopedUploads
                ON OutsiderLevels.UploadId = #ScopedUploads.Id;

                -- Drop the temp table
                DROP TABLE #ScopedUploads;";
            using (var command = databaseCommandFactory.Create(
                GetProgressDataCommandText,
                parameters))
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    // The DateTime is a datetime2 which has no timezone so comes out as DateTimeKind.Unknown. Se need to specify the kind so it gets serialized correctly.
                    var uploadTime = DateTime.SpecifyKind(Convert.ToDateTime(reader["UploadTime"]), DateTimeKind.Utc);

                    progressData.TitanDamageData.AddOrUpdate(uploadTime, reader["TitanDamage"].ToString());
                    progressData.SoulsSpentData.AddOrUpdate(uploadTime, reader["SoulsSpent"].ToString());
                    progressData.HeroSoulsSacrificedData.AddOrUpdate(uploadTime, reader["HeroSoulsSacrificed"].ToString());
                    progressData.TotalAncientSoulsData.AddOrUpdate(uploadTime, reader["TotalAncientSouls"].ToString());
                    progressData.TranscendentPowerData.AddOrUpdate(uploadTime, (100 * Convert.ToDouble(reader["TranscendentPower"])).ToString());
                    progressData.RubiesData.AddOrUpdate(uploadTime, reader["Rubies"].ToString());
                    progressData.HighestZoneThisTranscensionData.AddOrUpdate(uploadTime, reader["HighestZoneThisTranscension"].ToString());
                    progressData.HighestZoneLifetimeData.AddOrUpdate(uploadTime, reader["HighestZoneLifetime"].ToString());
                    progressData.AscensionsThisTranscensionData.AddOrUpdate(uploadTime, reader["AscensionsThisTranscension"].ToString());
                    progressData.AscensionsLifetimeData.AddOrUpdate(uploadTime, reader["AscensionsLifetime"].ToString());
                }

                if (!reader.NextResult())
                {
                    return null;
                }

                while (reader.Read())
                {
                    // The DateTime is a datetime2 which has no timezone so comes out as DateTimeKind.Unknown. Se need to specify the kind so it gets serialized correctly.
                    var uploadTime = DateTime.SpecifyKind(Convert.ToDateTime(reader["UploadTime"]), DateTimeKind.Utc);
                    var ancientId = Convert.ToInt32(reader["AncientId"]);
                    var level = reader["Level"].ToString();

                    if (!gameData.Ancients.TryGetValue(ancientId, out var ancient))
                    {
                        telemetryClient.TrackEvent("Unknown Ancient", new Dictionary<string, string> { { "AncientId", ancientId.ToString() } });
                        continue;
                    }

                    if (!progressData.AncientLevelData.TryGetValue(ancient.Name, out var levelData))
                    {
                        levelData = new SortedDictionary<DateTime, string>();
                        progressData.AncientLevelData.Add(ancient.Name, levelData);
                    }

                    levelData.AddOrUpdate(uploadTime, level);
                }

                if (!reader.NextResult())
                {
                    return null;
                }

                while (reader.Read())
                {
                    // The DateTime is a datetime2 which has no timezone so comes out as DateTimeKind.Unknown. Se need to specify the kind so it gets serialized correctly.
                    var uploadTime = DateTime.SpecifyKind(Convert.ToDateTime(reader["UploadTime"]), DateTimeKind.Utc);
                    var outsiderId = Convert.ToInt32(reader["OutsiderId"]);
                    var level = reader["Level"].ToString();

                    if (!gameData.Outsiders.TryGetValue(outsiderId, out var outsider))
                    {
                        telemetryClient.TrackEvent("Unknown Outsider", new Dictionary<string, string> { { "OutsiderId", outsiderId.ToString() } });
                        continue;
                    }

                    if (!progressData.OutsiderLevelData.TryGetValue(outsider.Name, out var levelData))
                    {
                        levelData = new SortedDictionary<DateTime, string>();
                        progressData.OutsiderLevelData.Add(outsider.Name, levelData);
                    }

                    levelData.AddOrUpdate(uploadTime, level);
                }
            }

            return progressData;
        }
    }
}