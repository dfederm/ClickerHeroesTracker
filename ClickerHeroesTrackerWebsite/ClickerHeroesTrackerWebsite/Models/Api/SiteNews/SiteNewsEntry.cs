// <copyright file="SiteNewsEntry.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Api.SiteNews
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a single site news entry
    /// </summary>
    public sealed class SiteNewsEntry
    {
        /// <summary>
        /// Gets or sets the date of the news entry
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the list of news messages
        /// </summary>
        public IList<string> Messages { get; set; }
    }
}