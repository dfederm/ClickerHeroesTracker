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
            IUserSettings userSettings,
            MiscellaneousStatsModel miscellaneousStatsModel)
        {
            this.SuggestedAncientLevels = GetSuggestions(gameData, savedGame, ancientLevels, miscellaneousStatsModel, userSettings);
        }

        /// <summary>
        /// Gets a collection of suggested ancient levels
        /// </summary>
        public KeyValuePair<int, double>[] SuggestedAncientLevels { get; }

        private static double GetCurrentAncientLevel(
            GameData gameData,
            IDictionary<Ancient, AncientLevelInfo> ancientLevels,
            bool useEffectiveLevel,
            int ancientId)
        {
            Ancient ancient;
            AncientLevelInfo levelInfo;
            return gameData.Ancients.TryGetValue(ancientId, out ancient)
                ? ancientLevels.TryGetValue(ancient, out levelInfo)
                    ? useEffectiveLevel
                        ? levelInfo.EffectiveLevel
                        : levelInfo.AncientLevel
                    : 0
                : 0;
        }

        private static KeyValuePair<int, double>[] GetSuggestions(
            GameData gameData,
            SavedGame savedGame,
            IDictionary<Ancient, AncientLevelInfo> ancientLevels,
            MiscellaneousStatsModel miscellaneousStatsModel,
            IUserSettings userSettings)
        {
            var useEffectiveLevel = userSettings.UseEffectiveLevelForSuggestions;
            var primaryAncientId = PrimaryAncients[userSettings.PlayStyle];

            var currentPrimaryAncientLevel = GetCurrentAncientLevel(gameData, ancientLevels, useEffectiveLevel, primaryAncientId);
            var currentArgaivLevel = GetCurrentAncientLevel(gameData, ancientLevels, useEffectiveLevel, AncientIds.Argaiv);
            var currentMorgLevel = GetCurrentAncientLevel(gameData, ancientLevels, useEffectiveLevel, AncientIds.Morgulis);
            var currentBubosLevel = GetCurrentAncientLevel(gameData, ancientLevels, useEffectiveLevel, AncientIds.Bubos);
            var currentChronosLevel = GetCurrentAncientLevel(gameData, ancientLevels, useEffectiveLevel, AncientIds.Chronos);
            var currentDoraLevel = GetCurrentAncientLevel(gameData, ancientLevels, useEffectiveLevel, AncientIds.Dora);
            var currentDogcogLevel = GetCurrentAncientLevel(gameData, ancientLevels, useEffectiveLevel, AncientIds.Dogcog);
            var currentFortunaLevel = GetCurrentAncientLevel(gameData, ancientLevels, useEffectiveLevel, AncientIds.Fortuna);
            var currentAtmanLevel = GetCurrentAncientLevel(gameData, ancientLevels, useEffectiveLevel, AncientIds.Atman);
            var currentKumaLevel = GetCurrentAncientLevel(gameData, ancientLevels, useEffectiveLevel, AncientIds.Kumawakamaru);
            var currentPhandoryssLevel = savedGame.OutsidersData?.Outsiders == null ? 0 : savedGame.OutsidersData.Outsiders.GetOutsiderLevel(OutsiderIds.Phandoryss);
            var ancientSoulsTotal = savedGame.AncientSoulsTotal;
            var highestFinishedZonePersist = savedGame.HighestFinishedZonePersist;
            var currentTp = miscellaneousStatsModel.TranscendentPower;

            var lnPrimary = Math.Log(currentPrimaryAncientLevel);
            var hpScale = 1.145 + (0.005 * Math.Floor(highestFinishedZonePersist / 500));
            var alpha = currentTp == 0 ? 0 : 1.4067 * Math.Log(1 + currentTp) / Math.Log(hpScale);
            var lnAlpha = currentTp == 0 ? 0 : Math.Log(alpha);

            // Common formulas across play styles
            var suggestedArgaivLevel = currentPrimaryAncientLevel;
            var suggestedMorgLevel = currentPrimaryAncientLevel * currentPrimaryAncientLevel;
            var suggestedBubosLevel = Math.Max(Math.Round((2.8 * lnPrimary) - (1.4 * Math.Log(1 + Math.Pow(Math.E, -0.02 * currentBubosLevel))) - 5.94), 0);
            var suggestedChronosLevel = Math.Max(Math.Round((2.75 * lnPrimary) - (1.375 * Math.Log(2 - Math.Pow(Math.E, -0.034 * currentChronosLevel))) - 5.1), 0);
            var suggestedGoldLevel = Math.Round(currentPrimaryAncientLevel * 0.926);
            var suggestedDoraLevel = Math.Max(Math.Round((2.877 * lnPrimary) - (1.4365 * Math.Log((100d / 99d) - Math.Pow(Math.E, -0.002 * currentDoraLevel))) - 9.63), 0);
            var suggestedDogcogLevel = Math.Max(Math.Round((2.844 * lnPrimary) - (1.422 * Math.Log((1d / 99d) + Math.Pow(Math.E, -0.01 * currentDogcogLevel))) - 7.232), 0);
            var suggestedFortunaLevel = Math.Max(Math.Round((2.875 * lnPrimary) - (1.4375 * Math.Log((10d / 9d) - Math.Pow(Math.E, -0.0025 * currentFortunaLevel))) - 9.3), 0);
            var suggestedSolomonLevel = savedGame.Transcendent
                ? Math.Round(Math.Pow(currentPrimaryAncientLevel, 0.8) / Math.Pow(alpha, 0.4))
                : GetPreTranscendentSuggestedSolomonLevel(gameData, ancientLevels, currentPrimaryAncientLevel, userSettings);
            var suggestedAtmanLevel = Math.Max(Math.Round((2.832 * lnPrimary) - (1.416 * lnAlpha) - (1.416 * Math.Log((4d / 3d) - Math.Pow(Math.E, -0.013 * currentAtmanLevel))) - 6.613), 0);
            var suggestedKumaLevel = Math.Max(Math.Round((2.844 * lnPrimary) - (1.422 * lnAlpha) - (1.422 * Math.Log(0.25 + Math.Pow(Math.E, -0.001 * currentKumaLevel))) - 7.014), 0);

            // Math per play style
            switch (userSettings.PlayStyle)
            {
                case PlayStyle.Idle:
                {
                    return new KeyValuePair<int, double>[]
                    {
                        new KeyValuePair<int, double>(AncientIds.Siyalatas, currentPrimaryAncientLevel),
                        new KeyValuePair<int, double>(AncientIds.Argaiv, suggestedArgaivLevel),
                        new KeyValuePair<int, double>(AncientIds.Morgulis, suggestedMorgLevel),
                        new KeyValuePair<int, double>(AncientIds.Bubos, suggestedBubosLevel),
                        new KeyValuePair<int, double>(AncientIds.Chronos, suggestedChronosLevel),
                        new KeyValuePair<int, double>(AncientIds.Libertas, suggestedGoldLevel),
                        new KeyValuePair<int, double>(AncientIds.Mammon, suggestedGoldLevel),
                        new KeyValuePair<int, double>(AncientIds.Mimzee, suggestedGoldLevel),
                        new KeyValuePair<int, double>(AncientIds.Dora, suggestedDoraLevel),
                        new KeyValuePair<int, double>(AncientIds.Dogcog, suggestedDogcogLevel),
                        new KeyValuePair<int, double>(AncientIds.Fortuna, suggestedFortunaLevel),
                        new KeyValuePair<int, double>(AncientIds.Solomon, suggestedSolomonLevel),
                        new KeyValuePair<int, double>(AncientIds.Atman, suggestedAtmanLevel),
                        new KeyValuePair<int, double>(AncientIds.Kumawakamaru, suggestedKumaLevel),
                    };
                }

                case PlayStyle.Hybrid:
                {
                    var hybridRatioReciprocal = 1d / userSettings.HybridRatio;
                    var suggestedActiveLevelUnrounded = hybridRatioReciprocal * currentPrimaryAncientLevel;
                    var suggestedActiveLevel = Math.Round(suggestedActiveLevelUnrounded);
                    var suggestedJuggernautLevel = Math.Round(Math.Pow(suggestedActiveLevelUnrounded, 0.8));
                    return new KeyValuePair<int, double>[]
                    {
                        new KeyValuePair<int, double>(AncientIds.Siyalatas, currentPrimaryAncientLevel),
                        new KeyValuePair<int, double>(AncientIds.Argaiv, suggestedArgaivLevel),
                        new KeyValuePair<int, double>(AncientIds.Morgulis, suggestedMorgLevel),
                        new KeyValuePair<int, double>(AncientIds.Bubos, suggestedBubosLevel),
                        new KeyValuePair<int, double>(AncientIds.Chronos, suggestedChronosLevel),
                        new KeyValuePair<int, double>(AncientIds.Libertas, suggestedGoldLevel),
                        new KeyValuePair<int, double>(AncientIds.Mammon, suggestedGoldLevel),
                        new KeyValuePair<int, double>(AncientIds.Mimzee, suggestedGoldLevel),
                        new KeyValuePair<int, double>(AncientIds.Dora, suggestedDoraLevel),
                        new KeyValuePair<int, double>(AncientIds.Dogcog, suggestedDogcogLevel),
                        new KeyValuePair<int, double>(AncientIds.Fortuna, suggestedFortunaLevel),
                        new KeyValuePair<int, double>(AncientIds.Solomon, suggestedSolomonLevel),
                        new KeyValuePair<int, double>(AncientIds.Atman, suggestedAtmanLevel),
                        new KeyValuePair<int, double>(AncientIds.Kumawakamaru, suggestedKumaLevel),
                        new KeyValuePair<int, double>(AncientIds.Fragsworth, suggestedActiveLevel),
                        new KeyValuePair<int, double>(AncientIds.Bhaal, suggestedActiveLevel),
                        new KeyValuePair<int, double>(AncientIds.Juggernaut, suggestedJuggernautLevel),
                    };
                }

                case PlayStyle.Active:
                {
                    var suggestedBhaalLevel = currentPrimaryAncientLevel;
                    var suggestedJuggernautLevel = Math.Round(Math.Pow(currentPrimaryAncientLevel, 0.8));
                    return new KeyValuePair<int, double>[]
                    {
                        new KeyValuePair<int, double>(AncientIds.Fragsworth, currentPrimaryAncientLevel),
                        new KeyValuePair<int, double>(AncientIds.Argaiv, suggestedArgaivLevel),
                        new KeyValuePair<int, double>(AncientIds.Morgulis, suggestedMorgLevel),
                        new KeyValuePair<int, double>(AncientIds.Bubos, suggestedBubosLevel),
                        new KeyValuePair<int, double>(AncientIds.Chronos, suggestedChronosLevel),
                        new KeyValuePair<int, double>(AncientIds.Bhaal, suggestedBhaalLevel),
                        new KeyValuePair<int, double>(AncientIds.Mammon, suggestedGoldLevel),
                        new KeyValuePair<int, double>(AncientIds.Mimzee, suggestedGoldLevel),
                        new KeyValuePair<int, double>(AncientIds.Dora, suggestedDoraLevel),
                        new KeyValuePair<int, double>(AncientIds.Dogcog, suggestedDogcogLevel),
                        new KeyValuePair<int, double>(AncientIds.Fortuna, suggestedFortunaLevel),
                        new KeyValuePair<int, double>(AncientIds.Solomon, suggestedSolomonLevel),
                        new KeyValuePair<int, double>(AncientIds.Atman, suggestedAtmanLevel),
                        new KeyValuePair<int, double>(AncientIds.Kumawakamaru, suggestedKumaLevel),
                        new KeyValuePair<int, double>(AncientIds.Juggernaut, suggestedJuggernautLevel),
                    };
                }

                default:
                {
                    throw new InvalidOperationException($"Unexpected Playstyle: {userSettings.PlayStyle}");
                }
            }
        }

        private static double GetPreTranscendentSuggestedSolomonLevel(
            GameData gameData,
            IDictionary<Ancient, AncientLevelInfo> ancientLevels,
            double currentPrimaryAncientLevel,
            IUserSettings userSettings)
        {
            var useEffectiveLevel = userSettings.UseEffectiveLevelForSuggestions;
            var solomonMultipliers = SolomonFormulaMultipliers[userSettings.PlayStyle];
            var solomonLogFunction = userSettings.UseReducedSolomonFormula
                ? (Func<double, double>)(d => Math.Log10(d))
                : (d => Math.Log(d));
            return currentPrimaryAncientLevel < 100
                ? currentPrimaryAncientLevel
                : Math.Round(solomonMultipliers[0] * Math.Pow(solomonLogFunction(solomonMultipliers[1] * Math.Pow(currentPrimaryAncientLevel, 2)), 0.4) * Math.Pow(currentPrimaryAncientLevel, 0.8));
        }
    }
}