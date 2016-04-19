// <copyright file="AncientLevelSummaryViewModel.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Calculator
{
    using System.Collections.Generic;
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
            SavedGame savedGame,
            TelemetryClient telemetryClient)
        {
            var ancientLevels = new SortedDictionary<Ancient, AncientLevelInfo>(AncientComparer.Instance);
            var itemLevelsById = savedGame.ItemsData.GetItemLevels();
            foreach (var ancientData in savedGame.AncientsData.Ancients.Values)
            {
                Ancient ancient;
                if (!gameData.Ancients.TryGetValue(ancientData.Id, out ancient))
                {
                    telemetryClient.TrackEvent("Unknown ancient", new Dictionary<string, string> { { "AncientId", ancientData.Id.ToString() } });
                    continue;
                }

                var ancientLevelInfo = new AncientLevelInfo(ancientData.Level, itemLevelsById.GetItemLevel(ancient.Id));
                ancientLevels.Add(ancient, ancientLevelInfo);
            }

            this.AncientLevels = ancientLevels;
        }

        /// <summary>
        /// Gets the levels for each ancient.
        /// </summary>
        public IDictionary<Ancient, AncientLevelInfo> AncientLevels { get; }

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