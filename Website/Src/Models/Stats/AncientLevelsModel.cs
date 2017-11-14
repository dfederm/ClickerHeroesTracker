// <copyright file="AncientLevelsModel.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Stats
{
    using System.Collections.Generic;
    using System.Numerics;
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
            var ancientLevels = new SortedDictionary<int, AncientLevelInfo>();
            var itemLevelsById = savedGame.ItemsData.GetItemLevels(gameData);
            foreach (var ancient in gameData.Ancients.Values)
            {
                // Skip ancients no longer in the game.
                if (ancient.NonTranscendent)
                {
                    continue;
                }

                var ancientLevel = savedGame.AncientsData.Ancients.TryGetValue(ancient.Id, out var ancientData)
                    ? ancientData.Level
                    : BigInteger.Zero;
                if (!itemLevelsById.TryGetValue(ancient.Id, out var itemLevel))
                {
                    itemLevel = BigInteger.Zero;
                }

                var ancientLevelInfo = new AncientLevelInfo(ancientLevel, itemLevel);
                ancientLevels.Add(ancient.Id, ancientLevelInfo);
            }

            this.AncientLevels = ancientLevels;
        }

        /// <summary>
        /// Gets the levels for each ancient.
        /// </summary>
        public IDictionary<int, AncientLevelInfo> AncientLevels { get; }
    }
}