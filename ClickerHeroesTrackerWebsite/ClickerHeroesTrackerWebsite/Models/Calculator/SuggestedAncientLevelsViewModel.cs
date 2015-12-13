// <copyright file="SuggestedAncientLevelsViewModel.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Calculator
{
    using System;
    using System.Collections.Generic;
    using Game;
    using Settings;

    /// <summary>
    /// The model for the suggested ancient levels view
    /// </summary>
    public class SuggestedAncientLevelsViewModel
    {
        private static readonly HashSet<PlayStyle> AllPlayStyles = new HashSet<PlayStyle>(new[]
        {
            PlayStyle.Idle,
            PlayStyle.Hybrid,
            PlayStyle.Active,
        });

        // Sources:
        // Idle: https://www.reddit.com/r/ClickerHeroes/comments/3823wt/mathematical_analysis_of_lategame_for_most_idle/
        // Active and Hybrid: https://www.reddit.com/r/ClickerHeroes/comments/3h5al8/extending_mathematical_analysis_to_hybrid_and/
        private static readonly HashSet<PlayStyle> HybridPlayStyles = new HashSet<PlayStyle>(new[]
        {
            PlayStyle.Hybrid,
        });

        private static readonly Dictionary<PlayStyle, double[]> SolomonFormulaMultipliers = new Dictionary<PlayStyle, double[]>
        {
            { PlayStyle.Idle, new[] { 1.15, 3.25 } },
            { PlayStyle.Active, new[] { 1.21, 3.73 } },
            { PlayStyle.Hybrid, new[] { 1.32, 4.65 } },
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="SuggestedAncientLevelsViewModel"/> class.
        /// </summary>
        public SuggestedAncientLevelsViewModel(
            IDictionary<Ancient, long> ancientLevels,
            int optimalLevel,
            IUserSettings userSettings)
        {
            this.UserSettings = userSettings;

            var currentSiyaLevel = GetCurrentAncientLevel(ancientLevels, Ancient.Siyalatas);
            var currentArgaivLevel = GetCurrentAncientLevel(ancientLevels, Ancient.Argaiv);
            var currentMorgLevel = GetCurrentAncientLevel(ancientLevels, Ancient.Morgulis);
            var currentLiberLevel = GetCurrentAncientLevel(ancientLevels, Ancient.Libertas);
            var currentMammonLevel = GetCurrentAncientLevel(ancientLevels, Ancient.Mammon);
            var currentMimzeeLevel = GetCurrentAncientLevel(ancientLevels, Ancient.Mimzee);
            var currentFragsworthLevel = GetCurrentAncientLevel(ancientLevels, Ancient.Fragsworth);
            var currentBhaalLevel = GetCurrentAncientLevel(ancientLevels, Ancient.Bhaal);
            var currentPlutoLevel = GetCurrentAncientLevel(ancientLevels, Ancient.Pluto);
            var currentJuggernautLevel = GetCurrentAncientLevel(ancientLevels, Ancient.Juggernaut);
            var currentIrisLevel = GetCurrentAncientLevel(ancientLevels, Ancient.Iris);
            var currentSolomonLevel = GetCurrentAncientLevel(ancientLevels, Ancient.Solomon);

            var suggestedSiyaLevel = currentSiyaLevel;
            var suggestedArgaivLevel = suggestedSiyaLevel + 9;
            var suggestedMorgLevel = (long)Math.Round(Math.Pow(suggestedSiyaLevel, 2) + (43.67 * suggestedSiyaLevel) + 33.58);
            var suggestedGoldLevel = (long)Math.Round(suggestedSiyaLevel * 0.93);
            var suggestedClickLevel = (long)Math.Round(suggestedSiyaLevel * 0.5);
            var suggestedJuggernautLevel = (long)Math.Round(Math.Pow(suggestedClickLevel, 0.8));
            var suggestedIrisLevel = optimalLevel - 1001;

            var solomonMultipliers = SolomonFormulaMultipliers[userSettings.PlayStyle];
            var solomonLogFunction = userSettings.UseReducedSolomonFormula
                ? (Func<double, double>)(d => Math.Log10(d))
                : (d => Math.Log(d));
            var suggestedSolomonLevel = (long)Math.Round(solomonMultipliers[0] * Math.Pow(solomonLogFunction(solomonMultipliers[1] * Math.Pow(suggestedSiyaLevel, 2)), 0.4) * Math.Pow(suggestedSiyaLevel, 0.8));

            this.SuggestedAncientLevels = new SuggestedAncientLevelData[]
            {
                new SuggestedAncientLevelData(Ancient.Siyalatas, currentSiyaLevel, suggestedSiyaLevel, AllPlayStyles),
                new SuggestedAncientLevelData(Ancient.Argaiv, currentArgaivLevel, suggestedArgaivLevel, AllPlayStyles),
                new SuggestedAncientLevelData(Ancient.Morgulis, currentMorgLevel, suggestedMorgLevel, AllPlayStyles),
                new SuggestedAncientLevelData(Ancient.Libertas, currentLiberLevel, suggestedGoldLevel, AllPlayStyles),
                new SuggestedAncientLevelData(Ancient.Mammon, currentMammonLevel, suggestedGoldLevel, AllPlayStyles),
                new SuggestedAncientLevelData(Ancient.Mimzee, currentMimzeeLevel, suggestedGoldLevel, AllPlayStyles),
                new SuggestedAncientLevelData(Ancient.Fragsworth, currentFragsworthLevel, suggestedClickLevel, HybridPlayStyles),
                new SuggestedAncientLevelData(Ancient.Bhaal, currentBhaalLevel, suggestedClickLevel, HybridPlayStyles),
                new SuggestedAncientLevelData(Ancient.Pluto, currentPlutoLevel, suggestedClickLevel, HybridPlayStyles),
                new SuggestedAncientLevelData(Ancient.Juggernaut, currentJuggernautLevel, suggestedJuggernautLevel, HybridPlayStyles),
                new SuggestedAncientLevelData(Ancient.Iris, currentIrisLevel, suggestedIrisLevel, AllPlayStyles),
                new SuggestedAncientLevelData(Ancient.Solomon, currentSolomonLevel, suggestedSolomonLevel, AllPlayStyles),
            };
        }

        /// <summary>
        /// Gets the current user's settings
        /// </summary>
        public IUserSettings UserSettings { get; }

        /// <summary>
        /// Gets a collection of suggested ancient levels
        /// </summary>
        public SuggestedAncientLevelData[] SuggestedAncientLevels { get; }

        private static long GetCurrentAncientLevel(IDictionary<Ancient, long> ancientLevels, Ancient ancient)
        {
            long level;
            return ancientLevels.TryGetValue(ancient, out level)
                ? level
                : 0;
        }

        /// <summary>
        /// A model that represents the suggested ancient level data for one ancient.
        /// </summary>
        public class SuggestedAncientLevelData
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SuggestedAncientLevelData"/> class.
            /// </summary>
            public SuggestedAncientLevelData(
                Ancient ancient,
                long currentLevel,
                long suggestedLevel,
                ISet<PlayStyle> supportedPlayStyles)
            {
                suggestedLevel = Math.Max(suggestedLevel, 0);

                this.AncientName = ancient.Name;
                this.CurrentLevel = currentLevel.ToString();
                this.SuggestedLevel = suggestedLevel.ToString();
                this.LevelDifference = (suggestedLevel - currentLevel).ToString();
                this.SupportedPlayStyles = supportedPlayStyles;
            }

            /// <summary>
            /// Gets the ancient name
            /// </summary>
            public string AncientName { get; }

            /// <summary>
            /// Gets the current ancient level
            /// </summary>
            public string CurrentLevel { get; }

            /// <summary>
            /// Gets the suggested ancient level
            /// </summary>
            public string SuggestedLevel { get; }

            /// <summary>
            /// Gets the difference in the suggested and current levels
            /// </summary>
            public string LevelDifference { get; }

            /// <summary>
            /// Gets the supported play styles for this suggestion.
            /// </summary>
            public ISet<PlayStyle> SupportedPlayStyles { get; }
        }
    }
}