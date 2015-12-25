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
        private static readonly Dictionary<PlayStyle, double[]> SolomonFormulaMultipliers = new Dictionary<PlayStyle, double[]>
        {
            { PlayStyle.Idle, new[] { 1.15, 3.25 } },
            { PlayStyle.Hybrid, new[] { 1.32, 4.65 } },
            { PlayStyle.Active, new[] { 1.21, 3.73 } },
        };

        private static readonly Dictionary<PlayStyle, int> PrimaryAncients = new Dictionary<PlayStyle, int>
        {
            { PlayStyle.Idle, AncientIds.Siyalatas },
            { PlayStyle.Hybrid, AncientIds.Siyalatas },
            { PlayStyle.Active, AncientIds.Fragsworth },
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

            this.PrimaryAncientId = PrimaryAncients[userSettings.PlayStyle];
            var currentPrimaryAncientLevel = GetCurrentAncientLevel(gameData, ancientLevels, this.PrimaryAncientId);

            // Common math
            var solomonMultipliers = SolomonFormulaMultipliers[userSettings.PlayStyle];
            var solomonLogFunction = userSettings.UseReducedSolomonFormula
                ? (Func<double, double>)(d => Math.Log10(d))
                : (d => Math.Log(d));
            var suggestedSolomonLevel = currentPrimaryAncientLevel < 100
                ? currentPrimaryAncientLevel
                : (long)Math.Round(solomonMultipliers[0] * Math.Pow(solomonLogFunction(solomonMultipliers[1] * Math.Pow(currentPrimaryAncientLevel, 2)), 0.4) * Math.Pow(currentPrimaryAncientLevel, 0.8));
            var suggestedIrisLevel = optimalLevel - 1001;

            // Math per play style
            switch (userSettings.PlayStyle)
            {
                case PlayStyle.Idle:
                {
                    var suggestedArgaivLevel = currentSiyaLevel < 100
                        ? currentSiyaLevel
                        : (currentSiyaLevel + 9);
                    var suggestedGoldLevel = (long)Math.Round(currentSiyaLevel * 0.927);
                    var suggestedMorgLevel = currentSiyaLevel < 100
                        ? (long)Math.Round(Math.Pow(currentSiyaLevel + 1, 2))
                        : (long)Math.Round(Math.Pow(currentSiyaLevel, 2) + (43.67 * currentSiyaLevel) + 33.58);

                    this.SuggestedAncientLevels = new SuggestedAncientLevelData[]
                    {
                        new SuggestedAncientLevelData(gameData, AncientIds.Siyalatas, currentSiyaLevel, currentSiyaLevel),
                        new SuggestedAncientLevelData(gameData, AncientIds.Argaiv, currentArgaivLevel, suggestedArgaivLevel),
                        new SuggestedAncientLevelData(gameData, AncientIds.Libertas, currentLiberLevel, suggestedGoldLevel),
                        new SuggestedAncientLevelData(gameData, AncientIds.Mammon, currentMammonLevel, suggestedGoldLevel),
                        new SuggestedAncientLevelData(gameData, AncientIds.Mimzee, currentMimzeeLevel, suggestedGoldLevel),
                        new SuggestedAncientLevelData(gameData, AncientIds.Morgulis, currentMorgLevel, suggestedMorgLevel),
                        new SuggestedAncientLevelData(gameData, AncientIds.Solomon, currentSolomonLevel, suggestedSolomonLevel),
                        new SuggestedAncientLevelData(gameData, AncientIds.Iris, currentIrisLevel, suggestedIrisLevel),
                    };

                    break;
                }

                case PlayStyle.Hybrid:
                {
                    var suggestedArgaivLevel = currentSiyaLevel < 100
                        ? currentSiyaLevel
                        : (currentSiyaLevel + 9);
                    var suggestedGoldLevel = (long)Math.Round(currentSiyaLevel * 0.927);
                    var suggestedClickLevel = (long)Math.Round(currentSiyaLevel * 0.5);
                    var suggestedJuggernautLevel = (long)Math.Round(Math.Pow(suggestedClickLevel, 0.8));
                    var suggestedMorgLevel = currentSiyaLevel < 100
                        ? (long)Math.Round(Math.Pow(currentSiyaLevel + 1, 2))
                        : (long)Math.Round(Math.Pow(currentSiyaLevel, 2) + (43.67 * currentSiyaLevel) + 33.58);

                    this.SuggestedAncientLevels = new SuggestedAncientLevelData[]
                    {
                        new SuggestedAncientLevelData(gameData, AncientIds.Siyalatas, currentSiyaLevel, currentSiyaLevel),
                        new SuggestedAncientLevelData(gameData, AncientIds.Argaiv, currentArgaivLevel, suggestedArgaivLevel),
                        new SuggestedAncientLevelData(gameData, AncientIds.Libertas, currentLiberLevel, suggestedGoldLevel),
                        new SuggestedAncientLevelData(gameData, AncientIds.Mammon, currentMammonLevel, suggestedGoldLevel),
                        new SuggestedAncientLevelData(gameData, AncientIds.Mimzee, currentMimzeeLevel, suggestedGoldLevel),
                        new SuggestedAncientLevelData(gameData, AncientIds.Bhaal, currentBhaalLevel, suggestedClickLevel),
                        new SuggestedAncientLevelData(gameData, AncientIds.Fragsworth, currentFragsworthLevel, suggestedClickLevel),
                        new SuggestedAncientLevelData(gameData, AncientIds.Pluto, currentPlutoLevel, suggestedClickLevel),
                        new SuggestedAncientLevelData(gameData, AncientIds.Juggernaut, currentJuggernautLevel, suggestedJuggernautLevel),
                        new SuggestedAncientLevelData(gameData, AncientIds.Morgulis, currentMorgLevel, suggestedMorgLevel),
                        new SuggestedAncientLevelData(gameData, AncientIds.Solomon, currentSolomonLevel, suggestedSolomonLevel),
                        new SuggestedAncientLevelData(gameData, AncientIds.Iris, currentIrisLevel, suggestedIrisLevel),
                    };

                    break;
                }

                case PlayStyle.Active:
                {
                    var suggestedArgaivLevel = currentFragsworthLevel;
                    var suggestedBhaalLevel = currentFragsworthLevel < 1000
                        ? currentFragsworthLevel
                        : (currentFragsworthLevel - 90);
                    var suggestedJuggernautLevel = (long)Math.Round(Math.Pow(currentFragsworthLevel, 0.8));
                    var suggestedPlutoLevel = (long)Math.Round(currentFragsworthLevel * 0.927);
                    var suggestedMorgLevel = currentFragsworthLevel < 100
                        ? (long)Math.Round(Math.Pow(currentFragsworthLevel + 1, 2))
                        : (long)Math.Round(Math.Pow(currentFragsworthLevel + 13, 2));

                    this.SuggestedAncientLevels = new SuggestedAncientLevelData[]
                    {
                        new SuggestedAncientLevelData(gameData, AncientIds.Fragsworth, currentFragsworthLevel, currentFragsworthLevel),
                        new SuggestedAncientLevelData(gameData, AncientIds.Argaiv, currentArgaivLevel, suggestedArgaivLevel),
                        new SuggestedAncientLevelData(gameData, AncientIds.Bhaal, currentBhaalLevel, suggestedBhaalLevel),
                        new SuggestedAncientLevelData(gameData, AncientIds.Pluto, currentPlutoLevel, suggestedPlutoLevel),
                        new SuggestedAncientLevelData(gameData, AncientIds.Juggernaut, currentJuggernautLevel, suggestedJuggernautLevel),
                        new SuggestedAncientLevelData(gameData, AncientIds.Morgulis, currentMorgLevel, suggestedMorgLevel),
                        new SuggestedAncientLevelData(gameData, AncientIds.Solomon, currentSolomonLevel, suggestedSolomonLevel),
                        ////new SuggestedAncientLevelData(gameData, AncientIds.Iris, currentIrisLevel, suggestedIrisLevel),
                    };

                    break;
                }
            }
        }

        /// <summary>
        /// Gets the current user's settings
        /// </summary>
        public IUserSettings UserSettings { get; }

        /// <summary>
        /// Gets the id of the primary ancient for the user's play style.
        /// </summary>
        public int PrimaryAncientId { get; }

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
                long suggestedLevel)
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
        }
    }
}