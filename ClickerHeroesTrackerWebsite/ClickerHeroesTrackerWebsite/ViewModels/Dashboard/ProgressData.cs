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
    using Microsoft.ApplicationInsights;
    using Settings;

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
            IDataReader reader,
            IUserSettings userSettings)
        {
            this.UserSettings = userSettings;

            this.OptimalLevelData = new SortedDictionary<DateTime, long>();
            this.SoulsPerHourData = new SortedDictionary<DateTime, long>();
            this.TitanDamageData = new SortedDictionary<DateTime, long>();
            this.SoulsSpentData = new SortedDictionary<DateTime, long>();

            while (reader.Read())
            {
                var uploadTime = Convert.ToDateTime(reader["UploadTime"]);
                this.OptimalLevelData.AddOrUpdate(uploadTime, Convert.ToInt64(reader["OptimalLevel"]));
                this.SoulsPerHourData.AddOrUpdate(uploadTime, Convert.ToInt64(reader["SoulsPerHour"]));
                this.TitanDamageData.AddOrUpdate(uploadTime, Convert.ToInt64(reader["TitanDamage"]));
                this.SoulsSpentData.AddOrUpdate(uploadTime, Convert.ToInt64(reader["SoulsSpent"]));
            }

            if (!reader.NextResult())
            {
                return;
            }

            this.AncientLevelData = new SortedDictionary<Ancient, IDictionary<DateTime, long>>(AncientComparer.Instance);
            while (reader.Read())
            {
                // UploadTime, AncientId, Level
                var uploadTime = Convert.ToDateTime(reader["UploadTime"]);
                var ancientId = Convert.ToInt32(reader["AncientId"]);
                var level = Convert.ToInt64(reader["Level"]);

                Ancient ancient;
                if (!gameData.Ancients.TryGetValue(ancientId, out ancient))
                {
                    telemetryClient.TrackEvent("Unknown Ancient", new Dictionary<string, string> { { "AncientId", ancientId.ToString() } });
                    continue;
                }

                // Skip ancients with max levels for now
                if (ancient.MaxLevel > 0)
                {
                    continue;
                }

                IDictionary<DateTime, long> levelData;
                if (!this.AncientLevelData.TryGetValue(ancient, out levelData))
                {
                    levelData = new SortedDictionary<DateTime, long>();
                    this.AncientLevelData.Add(ancient, levelData);
                }

                levelData.AddOrUpdate(uploadTime, level);
            }

            this.IsValid = true;
        }

        /// <summary>
        /// Gets a value indicating whether the model is valid.
        /// </summary>
        public bool IsValid { get; }

        /// <summary>
        /// Gets the curent user's settings.
        /// </summary>
        public IUserSettings UserSettings { get; }

        /// <summary>
        /// Gets the optimal level data, keyed on upload time.
        /// </summary>
        public IDictionary<DateTime, long> OptimalLevelData { get; }

        /// <summary>
        /// Gets the souls per hour data, keyed on upload time.
        /// </summary>
        public IDictionary<DateTime, long> SoulsPerHourData { get; }

        /// <summary>
        /// Gets the titan damage data, keyed on upload time.
        /// </summary>
        public IDictionary<DateTime, long> TitanDamageData { get; }

        /// <summary>
        /// Gets the souls spent data, keyed on upload time.
        /// </summary>
        public IDictionary<DateTime, long> SoulsSpentData { get; }

        /// <summary>
        /// Gets the ancient level data, keyed on ancient. The inner dictionaries are keyed on upload time.
        /// </summary>
        public IDictionary<Ancient, IDictionary<DateTime, long>> AncientLevelData { get; }

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