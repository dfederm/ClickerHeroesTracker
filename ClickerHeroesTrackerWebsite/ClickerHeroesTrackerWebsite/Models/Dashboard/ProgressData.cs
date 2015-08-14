namespace ClickerHeroesTrackerWebsite.Models.Dashboard
{
    using ClickerHeroesTrackerWebsite.Models.Game;
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;

    public class ProgressData
    {
        public ProgressData(SqlDataReader reader, UserSettings userSettings)
        {
            this.UserSettings = userSettings;

            this.OptimalLevelData = new SortedDictionary<DateTime, int>();
            this.SoulsPerHourData = new SortedDictionary<DateTime, int>();

            while (reader.Read())
            {
                var uploadTime = (DateTime)reader["UploadTime"];
                this.OptimalLevelData.Add(uploadTime, (short)reader["OptimalLevel"]);
                this.SoulsPerHourData.Add(uploadTime, (int)reader["SoulsPerHour"]);
            }

            if (!reader.NextResult())
            {
                return;
            }

            this.AncientLevelData = new SortedDictionary<Ancient, IDictionary<DateTime, int>>(AncientComparer.Instance);
            while (reader.Read())
            {
                // UploadTime, AncientId, Level
                var uploadTime = (DateTime)reader["UploadTime"];
                var ancientId = (byte)reader["AncientId"];
                var level = (int)reader["Level"];

                var ancient = Ancient.Get(ancientId);

                // Skip ancients with max levels for now
                if (ancient.MaxLevel > 0)
                {
                    continue;
                }

                IDictionary<DateTime, int> levelData;
                if (!this.AncientLevelData.TryGetValue(ancient, out levelData))
                {
                    levelData = new SortedDictionary<DateTime, int>();
                    this.AncientLevelData.Add(ancient, levelData);
                }

                levelData.Add(uploadTime, level);
            }

            this.IsValid = true;
        }

        public bool IsValid { get; private set; }

        public UserSettings UserSettings { get; private set; }

        public IDictionary<DateTime, int> OptimalLevelData { get; private set; }

        public IDictionary<DateTime, int> SoulsPerHourData { get; private set; }

        public IDictionary<Ancient, IDictionary<DateTime, int>> AncientLevelData { get; private set; }

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