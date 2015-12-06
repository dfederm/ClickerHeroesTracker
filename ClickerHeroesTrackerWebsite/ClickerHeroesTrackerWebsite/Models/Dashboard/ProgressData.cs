// <copyright file="ProgressData.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Dashboard
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using ClickerHeroesTrackerWebsite.Models;
    using ClickerHeroesTrackerWebsite.Models.Game;
    using Settings;

    public class ProgressData
    {
        public ProgressData(SqlDataReader reader, IUserSettings userSettings)
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

                var ancient = Ancient.Get(ancientId);

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

        public bool IsValid { get; private set; }

        public IUserSettings UserSettings { get; private set; }

        public IDictionary<DateTime, long> OptimalLevelData { get; private set; }

        public IDictionary<DateTime, long> SoulsPerHourData { get; private set; }

        public IDictionary<DateTime, long> TitanDamageData { get; private set; }

        public IDictionary<DateTime, long> SoulsSpentData { get; private set; }

        public IDictionary<Ancient, IDictionary<DateTime, long>> AncientLevelData { get; private set; }

        private class AncientComparer : IComparer<Ancient>
        {
            public static AncientComparer Instance = new AncientComparer();

            public int Compare(Ancient x, Ancient y)
            {
                return x.Name.CompareTo(y.Name);
            }
        }
    }
}