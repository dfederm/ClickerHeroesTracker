// <copyright file="PaginationMetadata.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Api
{
    public sealed class PaginationMetadata
    {
        public int Count { get; set; }

        public string Previous { get; set; }

        public string Next { get; set; }
    }
}