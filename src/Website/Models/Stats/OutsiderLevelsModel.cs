// <copyright file="OutsiderLevelsModel.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Stats
{
    using System.Collections.Generic;
    using ClickerHeroesTrackerWebsite.Models.SaveData;
    using Microsoft.ApplicationInsights;

    /// <summary>
    /// The model for the outsider level summary view.
    /// </summary>
    public class OutsiderLevelsModel
    {
        // BUGBUG 119 - Get real game data
        private static Dictionary<int, string> outsiderNames = new Dictionary<int, string>
        {
            { 1, "Xyliqil" },
            { 2, "Chor'gorloth" },
            { 3, "Phandoryss" },
            { 4, "Borb" },
            { 5, "Ponyboy" },
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="OutsiderLevelsModel"/> class.
        /// </summary>
        public OutsiderLevelsModel(
            SavedGame savedGame,
            TelemetryClient telemetryClient)
        {
            if (savedGame.OutsidersData != null && savedGame.OutsidersData.Outsiders != null)
            {
                foreach (var outsiderData in savedGame.OutsidersData.Outsiders.Values)
                {
                    string outsider;
                    if (!outsiderNames.TryGetValue(outsiderData.Id, out outsider))
                    {
                        telemetryClient.TrackEvent("Unknown outsider", new Dictionary<string, string> { { "OutsiderId", outsiderData.Id.ToString() } });
                        continue;
                    }

                    this.OutsiderLevels.Add(outsiderData.Id, new OutsiderLevelInfo(outsider, outsiderData.Level));
                }
            }
        }

        /// <summary>
        /// Gets the levels for each outsider.
        /// </summary>
        public IDictionary<int, OutsiderLevelInfo> OutsiderLevels { get; } = new SortedDictionary<int, OutsiderLevelInfo>();
    }
}