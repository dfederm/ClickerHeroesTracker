// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System.Collections.Generic;
using System.Numerics;
using ClickerHeroesTrackerWebsite.Models.Game;
using ClickerHeroesTrackerWebsite.Models.SaveData;
using ClickerHeroesTrackerWebsite.Utility;

namespace ClickerHeroesTrackerWebsite.Models.Stats
{
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
            SavedGame savedGame)
        {
            Dictionary<int, BigInteger> ancientLevels = new();
            Newtonsoft.Json.Linq.JToken ancientsData = savedGame.Object["ancients"]["ancients"];
            foreach (Ancient ancient in gameData.Ancients.Values)
            {
                // Skip ancients no longer in the game.
                if (ancient.NonTranscendent)
                {
                    continue;
                }

                BigInteger ancientLevel = ancientsData[ancient.Id.ToString()]?.Value<string>("level")?.ToBigInteger() ?? BigInteger.Zero;

                ancientLevels.Add(ancient.Id, ancientLevel);
            }

            AncientLevels = ancientLevels;
        }

        /// <summary>
        /// Gets the levels for each ancient.
        /// </summary>
        public IDictionary<int, BigInteger> AncientLevels { get; }
    }
}