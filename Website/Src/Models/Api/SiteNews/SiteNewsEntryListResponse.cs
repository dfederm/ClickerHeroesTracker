// <copyright file="SiteNewsEntryListResponse.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Api.SiteNews
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a site news entry list response
    /// </summary>
    public sealed class SiteNewsEntryListResponse
    {
        /// <summary>
        /// Gets or sets the list of uploads requested.
        /// </summary>
        public IDictionary<DateTime, IList<string>> Entries { get; set; }
    }
}