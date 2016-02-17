// <copyright file="AncientLevelSummaryViewModel.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Calculator
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using Game;
    using Microsoft.ApplicationInsights;
    using SaveData;

    /// <summary>
    /// The model for the ancient level summary view.
    /// </summary>
    public class AncientLevelSummaryViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AncientLevelSummaryViewModel"/> class.
        /// </summary>
        public AncientLevelSummaryViewModel(
            GameData gameData,
            AncientsData ancientsData,
            TelemetryClient telemetryClient)
        {
            var ancientLevels = new SortedDictionary<Ancient, long>(AncientComparer.Instance);
            foreach (var ancientData in ancientsData.Ancients.Values)
            {
                Ancient ancient;
                if (!gameData.Ancients.TryGetValue(ancientData.Id, out ancient))
                {
                    telemetryClient.TrackEvent("Unknown ancient", new Dictionary<string, string> { { "AncientId", ancientData.Id.ToString() } });
                    continue;
                }

                ancientLevels.Add(ancient, ancientData.Level);
            }

            this.AncientLevels = ancientLevels;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AncientLevelSummaryViewModel"/> class.
        /// </summary>
        public AncientLevelSummaryViewModel(
            GameData gameData,
            IDataReader reader,
            TelemetryClient telemetryClient)
        {
            var ancientLevels = new SortedDictionary<Ancient, long>(AncientComparer.Instance);
            while (reader.Read())
            {
                var ancientId = Convert.ToInt32(reader["AncientId"]);
                var level = Convert.ToInt64(reader["Level"]);

                Ancient ancient;
                if (!gameData.Ancients.TryGetValue(ancientId, out ancient))
                {
                    telemetryClient.TrackEvent("Unknown ancient", new Dictionary<string, string> { { "AncientId", ancientId.ToString() } });
                    continue;
                }

                ancientLevels.Add(ancient, level);
            }

            this.AncientLevels = ancientLevels;
        }

        /// <summary>
        /// Gets the levels for each ancient.
        /// </summary>
        public IDictionary<Ancient, long> AncientLevels { get; }

        private class AncientComparer : IComparer<Ancient>
        {
            private static AncientComparer instance = new AncientComparer();

            public static AncientComparer Instance
            {
                get
                {
                    return instance;
                }
            }

            public int Compare(Ancient x, Ancient y)
            {
                return x.Name.CompareTo(y.Name);
            }
        }
    }
}