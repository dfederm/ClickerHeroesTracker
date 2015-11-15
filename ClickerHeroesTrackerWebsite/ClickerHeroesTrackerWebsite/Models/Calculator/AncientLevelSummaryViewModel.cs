namespace ClickerHeroesTrackerWebsite.Models.Calculator
{
    using Game;
    using SaveData;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System;

    public class AncientLevelSummaryViewModel
    {
        public AncientLevelSummaryViewModel(AncientsData ancientsData)
        {
            var ancientLevels = new SortedDictionary<Ancient, long>(AncientComparer.Instance);
            foreach (var ancientData in ancientsData.Ancients.Values)
            {
                var ancient = Ancient.Get(ancientData.Id);
                if (ancient == null)
                {
                    // An ancient we didn't know about?
                    continue;
                }

                ancientLevels.Add(ancient, ancientData.Level);
            }

            this.AncientLevels = ancientLevels;
        }

        public AncientLevelSummaryViewModel(SqlDataReader reader)
        {
            var ancientLevels = new SortedDictionary<Ancient, long>(AncientComparer.Instance);
            while (reader.Read())
            {
                var ancientId = Convert.ToInt32(reader["AncientId"]);
                var level = Convert.ToInt64(reader["Level"]);

                var ancient = Ancient.Get(ancientId);

                ancientLevels.Add(ancient, level);
            }

            this.AncientLevels = ancientLevels;
        }

        public IDictionary<Ancient, long> AncientLevels { get; private set; }

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