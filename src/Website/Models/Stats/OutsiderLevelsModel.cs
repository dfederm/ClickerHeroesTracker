// <copyright file="OutsiderLevelsModel.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Stats
{
    using System.Collections.Generic;
    using ClickerHeroesTrackerWebsite.Models.Game;
    using ClickerHeroesTrackerWebsite.Models.SaveData;
    using Microsoft.ApplicationInsights;

    /// <summary>
    /// The model for the outsider level summary view.
    /// </summary>
    public class OutsiderLevelsModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OutsiderLevelsModel"/> class.
        /// </summary>
        public OutsiderLevelsModel(
            GameData gameData,
            SavedGame savedGame,
            TelemetryClient telemetryClient)
        {
            var outsiderLevels = new SortedDictionary<int, OutsiderLevelInfo>();
            foreach (var outsider in gameData.Outsiders.Values)
            {
                OutsiderData outsiderData = null;
                var outsiderLevel = (savedGame?.OutsidersData?.Outsiders?.TryGetValue(outsider.Id, out outsiderData)).GetValueOrDefault()
                    ? outsiderData.Level
                    : 0;
                var outsiderLevelInfo = new OutsiderLevelInfo(outsider.Name, outsiderLevel);
                outsiderLevels.Add(outsider.Id, outsiderLevelInfo);
            }

            this.OutsiderLevels = outsiderLevels;
        }

        /// <summary>
        /// Gets the levels for each outsider.
        /// </summary>
        public IDictionary<int, OutsiderLevelInfo> OutsiderLevels { get; } = new SortedDictionary<int, OutsiderLevelInfo>();
    }
}