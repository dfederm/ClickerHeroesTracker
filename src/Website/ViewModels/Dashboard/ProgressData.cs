// <copyright file="ProgressData.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Dashboard
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using ClickerHeroesTrackerWebsite.Models;
    using ClickerHeroesTrackerWebsite.Models.Game;
    using ClickerHeroesTrackerWebsite.Services.Database;
    using Microsoft.ApplicationInsights;

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
            DateTime? startTime,
            DateTime? endTime)
        {
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
	            SELECT #ScopedUploads.UploadTime, ComputedStats.TitanDamage, ComputedStats.SoulsSpent
	            FROM ComputedStats
	            INNER JOIN #ScopedUploads
	            ON ComputedStats.UploadId = #ScopedUploads.Id;

	            -- Ancient Levels
	            SELECT #ScopedUploads.UploadTime, AncientLevels.AncientId, AncientLevels.Level
	            FROM AncientLevels
	            INNER JOIN #ScopedUploads
	            ON AncientLevels.UploadId = #ScopedUploads.Id;

	            -- Drop the temp table
	            DROP TABLE #ScopedUploads;";
            using (var command = databaseCommandFactory.Create(
                GetProgressDataCommandText,
                parameters))
            using (var reader = command.ExecuteReader())
            {
                this.TitanDamageData = new SortedDictionary<DateTime, double>();
                this.SoulsSpentData = new SortedDictionary<DateTime, double>();

                while (reader.Read())
                {
                    var uploadTime = Convert.ToDateTime(reader["UploadTime"]);
                    this.TitanDamageData.AddOrUpdate(uploadTime, Convert.ToDouble(reader["TitanDamage"]));
                    this.SoulsSpentData.AddOrUpdate(uploadTime, Convert.ToDouble(reader["SoulsSpent"]));
                }

                if (!reader.NextResult())
                {
                    return;
                }

                this.AncientLevelData = new SortedDictionary<Ancient, IDictionary<DateTime, double>>(AncientComparer.Instance);
                while (reader.Read())
                {
                    // UploadTime, AncientId, Level
                    var uploadTime = Convert.ToDateTime(reader["UploadTime"]);
                    var ancientId = Convert.ToInt32(reader["AncientId"]);
                    var level = Convert.ToDouble(reader["Level"]);

                    Ancient ancient;
                    if (!gameData.Ancients.TryGetValue(ancientId, out ancient))
                    {
                        telemetryClient.TrackEvent("Unknown Ancient", new Dictionary<string, string> { { "AncientId", ancientId.ToString() } });
                        continue;
                    }

                    IDictionary<DateTime, double> levelData;
                    if (!this.AncientLevelData.TryGetValue(ancient, out levelData))
                    {
                        levelData = new SortedDictionary<DateTime, double>();
                        this.AncientLevelData.Add(ancient, levelData);
                    }

                    levelData.AddOrUpdate(uploadTime, level);
                }
            }

            this.IsValid = true;
        }

        /// <summary>
        /// Gets a value indicating whether the model is valid.
        /// </summary>
        public bool IsValid { get; }

        /// <summary>
        /// Gets the titan damage data, keyed on upload time.
        /// </summary>
        public IDictionary<DateTime, double> TitanDamageData { get; }

        /// <summary>
        /// Gets the souls spent data, keyed on upload time.
        /// </summary>
        public IDictionary<DateTime, double> SoulsSpentData { get; }

        /// <summary>
        /// Gets the ancient level data, keyed on ancient. The inner dictionaries are keyed on upload time.
        /// </summary>
        public IDictionary<Ancient, IDictionary<DateTime, double>> AncientLevelData { get; }

        private class AncientComparer : IComparer<Ancient>
        {
            public static AncientComparer Instance { get; } = new AncientComparer();

            public int Compare(Ancient x, Ancient y)
            {
                return x.Name.CompareTo(y.Name);
            }
        }
    }
}