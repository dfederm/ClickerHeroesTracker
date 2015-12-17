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
            GameData gameData,
            IDictionary<Ancient, long> ancientLevels,
            int optimalLevel,
            IUserSettings userSettings)
        {
            this.UserSettings = userSettings;

            var currentSiyaLevel = GetCurrentAncientLevel(gameData, ancientLevels, AncientIds.Siyalatas);
            var currentArgaivLevel = GetCurrentAncientLevel(gameData, ancientLevels, AncientIds.Argaiv);
            var currentMorgLevel = GetCurrentAncientLevel(gameData, ancientLevels, AncientIds.Morgulis);
            var currentLiberLevel = GetCurrentAncientLevel(gameData, ancientLevels, AncientIds.Libertas);
            var currentMammonLevel = GetCurrentAncientLevel(gameData, ancientLevels, AncientIds.Mammon);
            var currentMimzeeLevel = GetCurrentAncientLevel(gameData, ancientLevels, AncientIds.Mimzee);
            var currentFragsworthLevel = GetCurrentAncientLevel(gameData, ancientLevels, AncientIds.Fragsworth);
            var currentBhaalLevel = GetCurrentAncientLevel(gameData, ancientLevels, AncientIds.Bhaal);
            var currentPlutoLevel = GetCurrentAncientLevel(gameData, ancientLevels, AncientIds.Pluto);
            var currentJuggernautLevel = GetCurrentAncientLevel(gameData, ancientLevels, AncientIds.Juggernaut);
            var currentIrisLevel = GetCurrentAncientLevel(gameData, ancientLevels, AncientIds.Iris);
            var currentSolomonLevel = GetCurrentAncientLevel(gameData, ancientLevels, AncientIds.Solomon);

            var suggestedSiyaLevel = currentSiyaLevel;
            var suggestedArgaivLevel = suggestedSiyaLevel + 9;
            var suggestedMorgLevel = (long)Math.Round(Math.Pow(suggestedSiyaLevel, 2) + (43.67 * suggestedSiyaLevel) + 33.58);
            var suggestedGoldLevel = (long)Math.Round(suggestedSiyaLevel * 0.93);
            var suggestedClickLevel = (long)Math.Round(suggestedSiyaLevel * 0.5);
            var suggestedJuggernautLevel = (long)Math.Round(Math.Pow(suggestedClickLevel, 0.8));

            int suggestedIrisLevel;
            if (userSettings.Prefer30MinuteRuns)
            {
                suggestedIrisLevel = optimalLevel - 1001;
            }
            else
            {
                ////// Always be between 201 and 300 below the optimal level, always at a multiple of 100, minus 1.
                ////suggestedIrisLevel = optimalLevel - (optimalLevel % 100) - 201;
                suggestedIrisLevel = optimalLevel - 1001;
            }

            var solomonMultipliers = SolomonFormulaMultipliers[userSettings.PlayStyle];
            var solomonLogFunction = userSettings.UseReducedSolomonFormula
                ? (Func<double, double>)(d => Math.Log10(d))
                : (d => Math.Log(d));
            var suggestedSolomonLevel = (long)Math.Round(solomonMultipliers[0] * Math.Pow(solomonLogFunction(solomonMultipliers[1] * Math.Pow(suggestedSiyaLevel, 2)), 0.4) * Math.Pow(suggestedSiyaLevel, 0.8));

            this.SuggestedAncientLevels = new SuggestedAncientLevelData[]
            {
                new SuggestedAncientLevelData(gameData, AncientIds.Siyalatas, currentSiyaLevel, suggestedSiyaLevel, AllPlayStyles),
                new SuggestedAncientLevelData(gameData, AncientIds.Argaiv, currentArgaivLevel, suggestedArgaivLevel, AllPlayStyles),
                new SuggestedAncientLevelData(gameData, AncientIds.Morgulis, currentMorgLevel, suggestedMorgLevel, AllPlayStyles),
                new SuggestedAncientLevelData(gameData, AncientIds.Libertas, currentLiberLevel, suggestedGoldLevel, AllPlayStyles),
                new SuggestedAncientLevelData(gameData, AncientIds.Mammon, currentMammonLevel, suggestedGoldLevel, AllPlayStyles),
                new SuggestedAncientLevelData(gameData, AncientIds.Mimzee, currentMimzeeLevel, suggestedGoldLevel, AllPlayStyles),
                new SuggestedAncientLevelData(gameData, AncientIds.Fragsworth, currentFragsworthLevel, suggestedClickLevel, HybridPlayStyles),
                new SuggestedAncientLevelData(gameData, AncientIds.Bhaal, currentBhaalLevel, suggestedClickLevel, HybridPlayStyles),
                new SuggestedAncientLevelData(gameData, AncientIds.Pluto, currentPlutoLevel, suggestedClickLevel, HybridPlayStyles),
                new SuggestedAncientLevelData(gameData, AncientIds.Juggernaut, currentJuggernautLevel, suggestedJuggernautLevel, HybridPlayStyles),
                new SuggestedAncientLevelData(gameData, AncientIds.Iris, currentIrisLevel, suggestedIrisLevel, AllPlayStyles),
                new SuggestedAncientLevelData(gameData, AncientIds.Solomon, currentSolomonLevel, suggestedSolomonLevel, AllPlayStyles),
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

        private static long GetCurrentAncientLevel(
            GameData gameData,
            IDictionary<Ancient, long> ancientLevels,
            int ancientId)
        {
            Ancient ancient;
            long level;
            return gameData.Ancients.TryGetValue(ancientId, out ancient)
                ? ancientLevels.TryGetValue(ancient, out level)
                    ? level
                    : 0
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
                GameData gameData,
                int ancientId,
                long currentLevel,
                long suggestedLevel,
                ISet<PlayStyle> supportedPlayStyles)
            {
                suggestedLevel = Math.Max(suggestedLevel, 0);

                Ancient ancient;
                this.AncientId = ancientId;
                this.AncientName = gameData.Ancients.TryGetValue(ancientId, out ancient)
                    ? ancient.Name
                    : "<Unknown>";
                this.CurrentLevel = currentLevel.ToString();
                this.SuggestedLevel = suggestedLevel.ToString();
                this.LevelDifference = (suggestedLevel - currentLevel).ToString();
                this.SupportedPlayStyles = supportedPlayStyles;
            }

            /// <summary>
            /// Gets the ancient id
            /// </summary>
            public int AncientId { get; }

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