// <copyright file="PaginationMetadata.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Api
{
    /// <summary>
    /// Data to help with pagination of data.
    /// </summary>
    public sealed class PaginationMetadata
    {
        /// <summary>
        /// Gets or sets the total number of results
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets a url to use to fetch the previous page, or null of there is no previous page.
        /// </summary>
        public string Previous { get; set; }

        /// <summary>
        /// Gets or sets a url to use to fetch the next page, or null of there is no next page.
        /// </summary>
        public string Next { get; set; }
    }
}