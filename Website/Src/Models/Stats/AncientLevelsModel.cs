// <copyright file="AncientLevelsModel.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Stats
{
    using System.Collections.Generic;
    using System.Numerics;
    using ClickerHeroesTrackerWebsite.Models.Game;
    using ClickerHeroesTrackerWebsite.Models.SaveData;
    using ClickerHeroesTrackerWebsite.Utility;

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
            var ancientLevels = new Dictionary<int, BigInteger>();
            var ancientsData = savedGame.Object["ancients"]["ancients"];
            foreach (var ancient in gameData.Ancients.Values)
            {
                // Skip ancients no longer in the game.
                if (ancient.NonTranscendent)
                {
                    continue;
                }

                var ancientLevel = ancientsData[ancient.Id.ToString()]?.Value<string>("level")?.ToBigInteger() ?? BigInteger.Zero;

                ancientLevels.Add(ancient.Id, ancientLevel);
            }

            this.AncientLevels = ancientLevels;
        }

        /// <summary>
        /// Gets the levels for each ancient.
        /// </summary>
        public IDictionary<int, BigInteger> AncientLevels { get; }
    }
}