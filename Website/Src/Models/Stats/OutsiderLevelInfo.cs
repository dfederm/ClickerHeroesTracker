// <copyright file="OutsiderLevelInfo.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Stats
{
    /// <summary>
    /// A class that represents the outsider level information.
    /// </summary>
    public sealed class OutsiderLevelInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OutsiderLevelInfo"/> class.
        /// </summary>
        /// <param name="outsiderLevel">The outsider level</param>
        public OutsiderLevelInfo(string name, long level)
        {
            this.Name = name;
            this.Level = level;
        }

        /// <summary>
        /// Gets the outsider name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the outsider level
        /// </summary>
        public long Level { get; }
    }
}
