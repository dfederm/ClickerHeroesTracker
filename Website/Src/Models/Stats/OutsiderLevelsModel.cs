// <copyright file="OutsiderLevelsModel.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Stats
{
    using System.Collections.Generic;
    using ClickerHeroesTrackerWebsite.Models.Game;
    using ClickerHeroesTrackerWebsite.Models.SaveData;

    /// <summary>
    /// The model for the outsider level summary view.
    /// </summary>
    public class OutsiderLevelsModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OutsiderLevelsModel"/> class.
        /// </summary>
        public OutsiderLevelsModel(
            GameData gameData,
            SavedGame savedGame)
        {
            var outsiderLevels = new Dictionary<int, long>();
            var outsidersData = savedGame.Object["outsiders"]?["outsiders"];
            foreach (var outsider in gameData.Outsiders.Values)
            {
                // Skip outsiders no longer in the game. Unfortunately there is no field that tells whether it's active
                if (outsider.Id == 4)
                {
                    continue;
                }

                var outsiderLevel = (outsidersData?[outsider.Id.ToString()]?.Value<long>("level")).GetValueOrDefault(0);
                outsiderLevels.Add(outsider.Id, outsiderLevel);
            }

            this.OutsiderLevels = outsiderLevels;
        }

        /// <summary>
        /// Gets the levels for each outsider.
        /// </summary>
        public IDictionary<int, long> OutsiderLevels { get; }
    }
}