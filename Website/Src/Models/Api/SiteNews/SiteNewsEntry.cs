// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClickerHeroesTrackerWebsite.Models.Api.SiteNews
{
    /// <summary>
    /// Represents a single site news entry.
    /// </summary>
    public sealed class SiteNewsEntry
    {
        /// <summary>
        /// Gets or sets the date of the news entry.
        /// </summary>
        [Required]
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the list of news messages.
        /// </summary>
        [Required]
        [MinLength(1)]
        public IList<string> Messages { get; set; }
    }
}