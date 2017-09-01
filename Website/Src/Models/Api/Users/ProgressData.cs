// <copyright file="ProgressData.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Api.Users
{
    using System;
    using System.Collections.Generic;
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
        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressData"/> class.
        /// </summary>
        public ProgressData(
            GameData gameData,
            TelemetryClient telemetryClient,
            IDatabaseCommandFactory databaseCommandFactory,
            string userId,
            DateTime startTime,
            DateTime endTime)
        {
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
            using (var reader = command.ExecuteReader())
            {
                this.TitanDamageData = new SortedDictionary<DateTime, string>();
                this.SoulsSpentData = new SortedDictionary<DateTime, string>();
                this.HeroSoulsSacrificedData = new SortedDictionary<DateTime, string>();
                this.TotalAncientSoulsData = new SortedDictionary<DateTime, string>();
                this.TranscendentPowerData = new SortedDictionary<DateTime, string>();
                this.RubiesData = new SortedDictionary<DateTime, string>();
                this.HighestZoneThisTranscensionData = new SortedDictionary<DateTime, string>();
                this.HighestZoneLifetimeData = new SortedDictionary<DateTime, string>();
                this.AscensionsThisTranscensionData = new SortedDictionary<DateTime, string>();
                this.AscensionsLifetimeData = new SortedDictionary<DateTime, string>();

                while (reader.Read())
                {
                    // The DateTime is a datetime2 which has no timezone so comes out as DateTimeKind.Unknown. Se need to specify the kind so it gets serialized correctly.
                    var uploadTime = DateTime.SpecifyKind(Convert.ToDateTime(reader["UploadTime"]), DateTimeKind.Utc);

                    this.TitanDamageData.AddOrUpdate(uploadTime, reader["TitanDamage"].ToString());
                    this.SoulsSpentData.AddOrUpdate(uploadTime, reader["SoulsSpent"].ToString());
                    this.HeroSoulsSacrificedData.AddOrUpdate(uploadTime, reader["HeroSoulsSacrificed"].ToString());
                    this.TotalAncientSoulsData.AddOrUpdate(uploadTime, reader["TotalAncientSouls"].ToString());
                    this.TranscendentPowerData.AddOrUpdate(uploadTime, (100 * Convert.ToDouble(reader["TranscendentPower"])).ToString());
                    this.RubiesData.AddOrUpdate(uploadTime, reader["Rubies"].ToString());
                    this.HighestZoneThisTranscensionData.AddOrUpdate(uploadTime, reader["HighestZoneThisTranscension"].ToString());
                    this.HighestZoneLifetimeData.AddOrUpdate(uploadTime, reader["HighestZoneLifetime"].ToString());
                    this.AscensionsThisTranscensionData.AddOrUpdate(uploadTime, reader["AscensionsThisTranscension"].ToString());
                    this.AscensionsLifetimeData.AddOrUpdate(uploadTime, reader["AscensionsLifetime"].ToString());
                }

                if (!reader.NextResult())
                {
                    return;
                }

                this.AncientLevelData = new SortedDictionary<string, IDictionary<DateTime, string>>(StringComparer.OrdinalIgnoreCase);
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

                    if (!this.AncientLevelData.TryGetValue(ancient.Name, out var levelData))
                    {
                        levelData = new SortedDictionary<DateTime, string>();
                        this.AncientLevelData.Add(ancient.Name, levelData);
                    }

                    levelData.AddOrUpdate(uploadTime, level);
                }

                if (!reader.NextResult())
                {
                    return;
                }

                this.OutsiderLevelData = new SortedDictionary<string, IDictionary<DateTime, string>>(StringComparer.OrdinalIgnoreCase);
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

                    if (!this.OutsiderLevelData.TryGetValue(outsider.Name, out var levelData))
                    {
                        levelData = new SortedDictionary<DateTime, string>();
                        this.OutsiderLevelData.Add(outsider.Name, levelData);
                    }

                    levelData.AddOrUpdate(uploadTime, level);
                }
            }

            this.IsValid = true;
        }

        [JsonIgnore]
        public bool IsValid { get; }

        public IDictionary<DateTime, string> TitanDamageData { get; }

        public IDictionary<DateTime, string> SoulsSpentData { get; }

        public IDictionary<DateTime, string> HeroSoulsSacrificedData { get; }

        public IDictionary<DateTime, string> TotalAncientSoulsData { get; }

        public IDictionary<DateTime, string> TranscendentPowerData { get; }

        public IDictionary<DateTime, string> RubiesData { get; }

        public IDictionary<DateTime, string> HighestZoneThisTranscensionData { get; }

        public IDictionary<DateTime, string> HighestZoneLifetimeData { get; }

        public IDictionary<DateTime, string> AscensionsThisTranscensionData { get; }

        public IDictionary<DateTime, string> AscensionsLifetimeData { get; }

        public IDictionary<string, IDictionary<DateTime, string>> AncientLevelData { get; }

        public IDictionary<string, IDictionary<DateTime, string>> OutsiderLevelData { get; }
    }
}