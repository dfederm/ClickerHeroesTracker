// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System.Collections.Generic;
using ClickerHeroesTrackerWebsite.Models.Game;
using ClickerHeroesTrackerWebsite.Models.SaveData;

namespace ClickerHeroesTrackerWebsite.Models.Stats
{
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
            Dictionary<int, long> outsiderLevels = new();
            Newtonsoft.Json.Linq.JToken outsidersData = savedGame.Object["outsiders"]?["outsiders"];
            foreach (Outsider outsider in gameData.Outsiders.Values)
            {
                // Skip outsiders no longer in the game. Unfortunately there is no field that tells whether it's active
                if (outsider.Id == 4)
                {
                    continue;
                }

                long outsiderLevel = (outsidersData?[outsider.Id.ToString()]?.Value<long>("level")).GetValueOrDefault(0);
                outsiderLevels.Add(outsider.Id, outsiderLevel);
            }

            OutsiderLevels = outsiderLevels;
        }

        /// <summary>
        /// Gets the levels for each outsider.
        /// </summary>
        public IDictionary<int, long> OutsiderLevels { get; }
    }
}