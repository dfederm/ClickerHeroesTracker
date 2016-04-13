// <copyright file="ExperimentalStatsViewModel.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Calculator
{
    using System;
    using System.Collections.Generic;
    using Game;
    using Settings;

    /// <summary>
    /// A model for the experimental stats view.
    /// </summary>
    public class ExperimentalStatsViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExperimentalStatsViewModel"/> class.
        /// </summary>
        public ExperimentalStatsViewModel(
            GameData gameData,
            IDictionary<Ancient, AncientLevelInfo> ancientLevels,
            IUserSettings userSettings)
        {
            Ancient siyalatas;
            if (gameData.Ancients.TryGetValue(AncientIds.Siyalatas, out siyalatas))
            {
                AncientLevelInfo siyaLevel;
                if (userSettings.PlayStyle == PlayStyle.Idle
                    && ancientLevels.TryGetValue(siyalatas, out siyaLevel))
                {
                    // Source: https://www.reddit.com/r/ClickerHeroes/comments/3f3djb/late_game_iris_and_other_large_numbers/
                    this.OptimalLevel = Math.Max(0, (int)Math.Round(371 * Math.Log(siyaLevel.EffectiveLevel)) - 1080);
                }
            }
        }

        /// <summary>
        /// Gets the user's optimal ascension level, based on formula.
        /// </summary>
        public int? OptimalLevel { get; }
    }
}