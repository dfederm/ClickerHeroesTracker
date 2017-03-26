// <copyright file="AncientLevelsModel.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Stats
{
    using System.Collections.Generic;
    using ClickerHeroesTrackerWebsite.Models.Game;
    using ClickerHeroesTrackerWebsite.Models.SaveData;
    using Microsoft.ApplicationInsights;

    /// <summary>
    /// The model for the ancient level summary view.
    /// </summary>
    public class AncientLevelsModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AncientLevelsModel"/> class.
        /// </summary>
        public AncientLevelsModel(
            GameData gameData,
            SavedGame savedGame,
            TelemetryClient telemetryClient)
        {
            var ancientLevels = new SortedDictionary<Ancient, AncientLevelInfo>(AncientComparer.Instance);
            var itemLevelsById = savedGame.ItemsData.GetItemLevels(gameData);
            foreach (var ancient in gameData.Ancients.Values)
            {
                // Skip ancients no longer in the game.
                if (ancient.NonTranscendent)
                {
                    continue;
                }

                AncientData ancientData;
                var ancientLevel = savedGame.AncientsData.Ancients.TryGetValue(ancient.Id, out ancientData)
                    ? ancientData.Level
                    : 0;
                var ancientLevelInfo = new AncientLevelInfo(ancientLevel, itemLevelsById.GetItemLevel(ancient.Id));
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