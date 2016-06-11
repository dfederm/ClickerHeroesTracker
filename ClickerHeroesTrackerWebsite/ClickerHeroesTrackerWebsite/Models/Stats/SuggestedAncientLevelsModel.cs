// <copyright file="SuggestedAncientLevelsModel.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Stats
{
    using System;
    using System.Collections.Generic;
    using ClickerHeroesTrackerWebsite.Models.Game;
    using ClickerHeroesTrackerWebsite.Models.SaveData;
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
            SavedGame savedGame,
            IDictionary<Ancient, AncientLevelInfo> ancientLevels,
            int optimalLevel,
            IUserSettings userSettings)
        {
            this.SuggestedAncientLevels = savedGame.OutsidersData == null
                ? GetLegacySuggestions(gameData, ancientLevels, optimalLevel, userSettings)
                : GetSuggestions(gameData, savedGame, ancientLevels);
        }

        /// <summary>
        /// Gets a collection of suggested ancient levels
        /// </summary>
        public KeyValuePair<int, long>[] SuggestedAncientLevels { get; }

        private static long GetCurrentAncientLevel(
            GameData gameData,
            IDictionary<Ancient, AncientLevelInfo> ancientLevels,
            int ancientId)
        {
            Ancient ancient;
            AncientLevelInfo levelInfo;
            return gameData.Ancients.TryGetValue(ancientId, out ancient)
                ? ancientLevels.TryGetValue(ancient, out levelInfo)
                    ? levelInfo.EffectiveLevel
                    : 0
                : 0;
        }

        private static KeyValuePair<int, long>[] GetSuggestions(
            GameData gameData,
            SavedGame savedGame,
            IDictionary<Ancient, AncientLevelInfo> ancientLevels)
        {
            var currentSiyaLevel = GetCurrentAncientLevel(gameData, ancientLevels, AncientIds.Siyalatas);
            var currentArgaivLevel = GetCurrentAncientLevel(gameData, ancientLevels, AncientIds.Argaiv);
            var currentMorgLevel = GetCurrentAncientLevel(gameData, ancientLevels, AncientIds.Morgulis);
            var currentBubosLevel = GetCurrentAncientLevel(gameData, ancientLevels, AncientIds.Bubos);
            var currenChronosLevel = GetCurrentAncientLevel(gameData, ancientLevels, AncientIds.Chronos);
            var currenDoraLevel = GetCurrentAncientLevel(gameData, ancientLevels, AncientIds.Dora);
            var currenDogcogLevel = GetCurrentAncientLevel(gameData, ancientLevels, AncientIds.Dogcog);
            var currenFortunaLevel = GetCurrentAncientLevel(gameData, ancientLevels, AncientIds.Fortuna);
            var currentAtmanLevel = GetCurrentAncientLevel(gameData, ancientLevels, AncientIds.Atman);
            var currentKumaLevel = GetCurrentAncientLevel(gameData, ancientLevels, AncientIds.Kumawakamaru);
            var currentPhandoryssLevel = savedGame.OutsidersData.Outsiders.GetOutsiderLevel(3); // BUGBUG 119 - Get real game data
            var ancientSoulsTotal = savedGame.AncientSoulsTotal;
            var highestFinishedZonePersist = savedGame.HighestFinishedZonePersist;

            var currentTp = (50 - (49 * Math.Pow(Math.E, -ancientSoulsTotal / 10000)) + (currentPhandoryssLevel * 0.05)) / 100;
            var lnSiya = Math.Log(currentSiyaLevel);
            var hpScale = 1.145 + (0.005 * Math.Floor(highestFinishedZonePersist / 500));
            var alpha = 1.4067 * Math.Log(1 + currentTp) / Math.Log(hpScale);
            var lnAlpha = Math.Log(alpha);

            var suggestedArgaivLevel = currentSiyaLevel;
            var suggestedMorgLevel = currentSiyaLevel * currentSiyaLevel;
            var suggestedBubosLevel = (long)Math.Round((2.8 * lnSiya) - (1.4 * Math.Log(1 + Math.Pow(Math.E, -0.02 * currentBubosLevel))) - 5.94);
            var suggestedChronosLevel = (long)Math.Round((2.75 * lnSiya) - (1.375 * Math.Log(2 - Math.Pow(Math.E, -0.034 * currenChronosLevel))) - 5.1);
            var suggestedGoldLevel = (long)Math.Round(currentSiyaLevel * 0.926);
            var suggestedDoraLevel = (long)Math.Round((2.877 * lnSiya) - (1.4365 * Math.Log((100d / 99d) - Math.Pow(Math.E, -0.002 * currenDoraLevel))) - 9.63);
            var suggestedDogcogLevel = (long)Math.Round((2.844 * lnSiya) - ((1d / 99d) + Math.Pow(Math.E, -0.01 * currenDogcogLevel)) - 7.232);
            var suggestedFortunaLevel = (long)Math.Round((2.875 * lnSiya) - (1.4375 * Math.Log((10d / 9d) - Math.Pow(Math.E, -0.0025 * currenFortunaLevel))) - 9.3);
            var suggestedSolomonLevel = (long)Math.Round(Math.Pow(currentSiyaLevel, 0.8) / Math.Pow(alpha, 0.4));
            var suggestedAtmanLevel = (long)Math.Round((2.832 * lnSiya) - (1.416 * lnAlpha) - (1.416 * Math.Log((4d / 3d) - Math.Pow(Math.E, -0.013 * currentAtmanLevel))) - 6.613);
            var suggestedKumaLevel = (long)Math.Round((2.88 * lnSiya) - (1.44 * lnAlpha) - (1.44 * Math.Log(0.25 + Math.Pow(Math.E, -0.001 * currentKumaLevel))) - 10.42);

            return new KeyValuePair<int, long>[]
            {
                new KeyValuePair<int, long>(AncientIds.Siyalatas, currentSiyaLevel),
                new KeyValuePair<int, long>(AncientIds.Argaiv, suggestedArgaivLevel),
                new KeyValuePair<int, long>(AncientIds.Morgulis, suggestedMorgLevel),
                new KeyValuePair<int, long>(AncientIds.Bubos, suggestedBubosLevel),
                new KeyValuePair<int, long>(AncientIds.Chronos, suggestedChronosLevel),
                new KeyValuePair<int, long>(AncientIds.Libertas, suggestedGoldLevel),
                new KeyValuePair<int, long>(AncientIds.Mammon, suggestedGoldLevel),
                new KeyValuePair<int, long>(AncientIds.Mimzee, suggestedGoldLevel),
                new KeyValuePair<int, long>(AncientIds.Dora, suggestedDoraLevel),
                new KeyValuePair<int, long>(AncientIds.Dogcog, suggestedDogcogLevel),
                new KeyValuePair<int, long>(AncientIds.Fortuna, suggestedFortunaLevel),
                new KeyValuePair<int, long>(AncientIds.Solomon, suggestedSolomonLevel),
                new KeyValuePair<int, long>(AncientIds.Atman, suggestedAtmanLevel),
                new KeyValuePair<int, long>(AncientIds.Kumawakamaru, suggestedKumaLevel),
            };
        }

        private static KeyValuePair<int, long>[] GetLegacySuggestions(
            GameData gameData,
            IDictionary<Ancient, AncientLevelInfo> ancientLevels,
            int optimalLevel,
            IUserSettings userSettings)
        {
            var currentSiyaLevel = GetCurrentAncientLevel(gameData, ancientLevels, AncientIds.Siyalatas);
            var currentFragsworthLevel = GetCurrentAncientLevel(gameData, ancientLevels, AncientIds.Fragsworth);

            var primaryAncientId = PrimaryAncients[userSettings.PlayStyle];
            var currentPrimaryAncientLevel = GetCurrentAncientLevel(gameData, ancientLevels, primaryAncientId);

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

                    return new KeyValuePair<int, long>[]
                    {
                        new KeyValuePair<int, long>(AncientIds.Siyalatas, currentSiyaLevel),
                        new KeyValuePair<int, long>(AncientIds.Argaiv, suggestedArgaivLevel),
                        new KeyValuePair<int, long>(AncientIds.Libertas, suggestedGoldLevel),
                        new KeyValuePair<int, long>(AncientIds.Mammon, suggestedGoldLevel),
                        new KeyValuePair<int, long>(AncientIds.Mimzee, suggestedGoldLevel),
                        new KeyValuePair<int, long>(AncientIds.Morgulis, suggestedMorgLevel),
                        new KeyValuePair<int, long>(AncientIds.Solomon, suggestedSolomonLevel),
                        new KeyValuePair<int, long>(AncientIds.Iris, suggestedIrisLevel),
                    };
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

                    return new KeyValuePair<int, long>[]
                    {
                        new KeyValuePair<int, long>(AncientIds.Siyalatas, currentSiyaLevel),
                        new KeyValuePair<int, long>(AncientIds.Argaiv, suggestedArgaivLevel),
                        new KeyValuePair<int, long>(AncientIds.Libertas, suggestedGoldLevel),
                        new KeyValuePair<int, long>(AncientIds.Mammon, suggestedGoldLevel),
                        new KeyValuePair<int, long>(AncientIds.Mimzee, suggestedGoldLevel),
                        new KeyValuePair<int, long>(AncientIds.Bhaal, suggestedClickLevel),
                        new KeyValuePair<int, long>(AncientIds.Fragsworth, suggestedClickLevel),
                        new KeyValuePair<int, long>(AncientIds.Pluto, suggestedClickLevel),
                        new KeyValuePair<int, long>(AncientIds.Juggernaut, suggestedJuggernautLevel),
                        new KeyValuePair<int, long>(AncientIds.Morgulis, suggestedMorgLevel),
                        new KeyValuePair<int, long>(AncientIds.Solomon, suggestedSolomonLevel),
                        new KeyValuePair<int, long>(AncientIds.Iris, suggestedIrisLevel),
                    };
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

                    return new KeyValuePair<int, long>[]
                    {
                        new KeyValuePair<int, long>(AncientIds.Fragsworth, currentFragsworthLevel),
                        new KeyValuePair<int, long>(AncientIds.Argaiv, suggestedArgaivLevel),
                        new KeyValuePair<int, long>(AncientIds.Bhaal, suggestedBhaalLevel),
                        new KeyValuePair<int, long>(AncientIds.Pluto, suggestedPlutoLevel),
                        new KeyValuePair<int, long>(AncientIds.Juggernaut, suggestedJuggernautLevel),
                        new KeyValuePair<int, long>(AncientIds.Morgulis, suggestedMorgLevel),
                        new KeyValuePair<int, long>(AncientIds.Solomon, suggestedSolomonLevel),
                        new KeyValuePair<int, long>(AncientIds.Iris, suggestedIrisLevel),
                    };
                }

                default:
                {
                    throw new InvalidOperationException($"Unexpected Playstyle: {userSettings.PlayStyle}");
                }
            }
        }
    }
}