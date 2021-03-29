// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System;
using System.Collections.Generic;

namespace ClickerHeroesTrackerWebsite.Models.Api.SiteNews
{
    /// <summary>
    /// Represents a site news entry list response.
    /// </summary>
    public sealed class SiteNewsEntryListResponse
    {
        /// <summary>
        /// Gets or sets the list of uploads requested.
        /// </summary>
        public IDictionary<DateTime, IList<string>> Entries { get; set; }
    }
}