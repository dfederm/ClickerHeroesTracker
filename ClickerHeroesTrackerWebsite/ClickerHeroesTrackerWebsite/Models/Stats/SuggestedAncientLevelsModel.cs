// <copyright file="SuggestedAncientLevelsModel.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Stats
{
    using System;
    using System.Collections.Generic;
    using ClickerHeroesTrackerWebsite.Models.Game;
    using ClickerHeroesTrackerWebsite.Models.Settings;

    /// <summary>
    /// The model for the suggested ancient levels view
    /// </summary>
    public class SuggestedAncientLevelsModel
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
        /// Initializes a new instance of the <see cref="SuggestedAncientLevelsModel"/> class.
        /// </summary>
        public SuggestedAncientLevelsModel(
            GameData gameData,
            IDictionary<Ancient, AncientLevelInfo> ancientLevels,
            int optimalLevel,
            IUserSettings userSettings)
        {
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

            var primaryAncientId = PrimaryAncients[userSettings.PlayStyle];
            var currentPrimaryAncientLevel = GetCurrentAncientLevel(gameData, ancientLevels, primaryAncientId);

            // Common math
            var solomonMultipliers = SolomonFormulaMultipliers[userSettings.PlayStyle];
            var solomonLogFunction = userSettings.UseReducedSolomonFormula
                ? (Func<double, double>)(d => Math.Log10(d))
                : (d => Math.Log(d));
            var suggestedSolomonLevel = currentPrimaryAncientLevel.EffectiveLevel < 100
                ? currentPrimaryAncientLevel.EffectiveLevel
                : (long)Math.Round(solomonMultipliers[0] * Math.Pow(solomonLogFunction(solomonMultipliers[1] * Math.Pow(currentPrimaryAncientLevel.EffectiveLevel, 2)), 0.4) * Math.Pow(currentPrimaryAncientLevel.EffectiveLevel, 0.8));
            var suggestedIrisLevel = optimalLevel - 1001;

            // Math per play style
            switch (userSettings.PlayStyle)
            {
                case PlayStyle.Idle:
                {
                    var suggestedArgaivLevel = currentSiyaLevel.EffectiveLevel < 100
                        ? currentSiyaLevel.EffectiveLevel
                        : (currentSiyaLevel.EffectiveLevel + 9);
                    var suggestedGoldLevel = (long)Math.Round(currentSiyaLevel.EffectiveLevel * 0.927);
                    var suggestedMorgLevel = currentSiyaLevel.EffectiveLevel < 100
                        ? (long)Math.Round(Math.Pow(currentSiyaLevel.EffectiveLevel + 1, 2))
                        : (long)Math.Round(Math.Pow(currentSiyaLevel.EffectiveLevel, 2) + (43.67 * currentSiyaLevel.EffectiveLevel) + 33.58);

                    this.SuggestedAncientLevels = new KeyValuePair<int, double>[]
                    {
                        new KeyValuePair<int, double>(AncientIds.Siyalatas, currentSiyaLevel.EffectiveLevel),
                        new KeyValuePair<int, double>(AncientIds.Argaiv, suggestedArgaivLevel),
                        new KeyValuePair<int, double>(AncientIds.Libertas, suggestedGoldLevel),
                        new KeyValuePair<int, double>(AncientIds.Mammon, suggestedGoldLevel),
                        new KeyValuePair<int, double>(AncientIds.Mimzee, suggestedGoldLevel),
                        new KeyValuePair<int, double>(AncientIds.Morgulis, suggestedMorgLevel),
                        new KeyValuePair<int, double>(AncientIds.Solomon, suggestedSolomonLevel),
                        new KeyValuePair<int, double>(AncientIds.Iris, suggestedIrisLevel),
                    };

                    break;
                }

                case PlayStyle.Hybrid:
                {
                    var suggestedArgaivLevel = currentSiyaLevel.EffectiveLevel < 100
                        ? currentSiyaLevel.EffectiveLevel
                        : (currentSiyaLevel.EffectiveLevel + 9);
                    var suggestedGoldLevel = (long)Math.Round(currentSiyaLevel.EffectiveLevel * 0.927);
                    var suggestedClickLevel = (long)Math.Round(currentSiyaLevel.EffectiveLevel * 0.5);
                    var suggestedJuggernautLevel = (long)Math.Round(Math.Pow(suggestedClickLevel, 0.8));
                    var suggestedMorgLevel = currentSiyaLevel.EffectiveLevel < 100
                        ? (long)Math.Round(Math.Pow(currentSiyaLevel.EffectiveLevel + 1, 2))
                        : (long)Math.Round(Math.Pow(currentSiyaLevel.EffectiveLevel, 2) + (43.67 * currentSiyaLevel.EffectiveLevel) + 33.58);

                    this.SuggestedAncientLevels = new KeyValuePair<int, double>[]
                    {
                        new KeyValuePair<int, double>(AncientIds.Siyalatas, currentSiyaLevel.EffectiveLevel),
                        new KeyValuePair<int, double>(AncientIds.Argaiv, suggestedArgaivLevel),
                        new KeyValuePair<int, double>(AncientIds.Libertas, suggestedGoldLevel),
                        new KeyValuePair<int, double>(AncientIds.Mammon, suggestedGoldLevel),
                        new KeyValuePair<int, double>(AncientIds.Mimzee, suggestedGoldLevel),
                        new KeyValuePair<int, double>(AncientIds.Bhaal, suggestedClickLevel),
                        new KeyValuePair<int, double>(AncientIds.Fragsworth, suggestedClickLevel),
                        new KeyValuePair<int, double>(AncientIds.Pluto, suggestedClickLevel),
                        new KeyValuePair<int, double>(AncientIds.Juggernaut, suggestedJuggernautLevel),
                        new KeyValuePair<int, double>(AncientIds.Morgulis, suggestedMorgLevel),
                        new KeyValuePair<int, double>(AncientIds.Solomon, suggestedSolomonLevel),
                        new KeyValuePair<int, double>(AncientIds.Iris, suggestedIrisLevel),
                    };

                    break;
                }

                case PlayStyle.Active:
                {
                    var suggestedArgaivLevel = currentFragsworthLevel.EffectiveLevel;
                    var suggestedBhaalLevel = currentFragsworthLevel.EffectiveLevel < 1000
                        ? currentFragsworthLevel.EffectiveLevel
                        : (currentFragsworthLevel.EffectiveLevel - 90);
                    var suggestedJuggernautLevel = (long)Math.Round(Math.Pow(currentFragsworthLevel.EffectiveLevel, 0.8));
                    var suggestedPlutoLevel = (long)Math.Round(currentFragsworthLevel.EffectiveLevel * 0.927);
                    var suggestedMorgLevel = currentFragsworthLevel.EffectiveLevel < 100
                        ? (long)Math.Round(Math.Pow(currentFragsworthLevel.EffectiveLevel + 1, 2))
                        : (long)Math.Round(Math.Pow(currentFragsworthLevel.EffectiveLevel + 13, 2));

                    this.SuggestedAncientLevels = new KeyValuePair<int, double>[]
                    {
                        new KeyValuePair<int, double>(AncientIds.Fragsworth, currentFragsworthLevel.EffectiveLevel),
                        new KeyValuePair<int, double>(AncientIds.Argaiv, suggestedArgaivLevel),
                        new KeyValuePair<int, double>(AncientIds.Bhaal, suggestedBhaalLevel),
                        new KeyValuePair<int, double>(AncientIds.Pluto, suggestedPlutoLevel),
                        new KeyValuePair<int, double>(AncientIds.Juggernaut, suggestedJuggernautLevel),
                        new KeyValuePair<int, double>(AncientIds.Morgulis, suggestedMorgLevel),
                        new KeyValuePair<int, double>(AncientIds.Solomon, suggestedSolomonLevel),
                        new KeyValuePair<int, double>(AncientIds.Iris, suggestedIrisLevel),
                    };

                    break;
                }
            }
        }

        /// <summary>
        /// Gets a collection of suggested ancient levels
        /// </summary>
        public KeyValuePair<int, double>[] SuggestedAncientLevels { get; }

        private static AncientLevelInfo GetCurrentAncientLevel(
            GameData gameData,
            IDictionary<Ancient, AncientLevelInfo> ancientLevels,
            int ancientId)
        {
            Ancient ancient;
            AncientLevelInfo levelInfo;
            return gameData.Ancients.TryGetValue(ancientId, out ancient)
                ? ancientLevels.TryGetValue(ancient, out levelInfo)
                    ? levelInfo
                    : new AncientLevelInfo(0, 0)
                : new AncientLevelInfo(0, 0);
        }
    }
}