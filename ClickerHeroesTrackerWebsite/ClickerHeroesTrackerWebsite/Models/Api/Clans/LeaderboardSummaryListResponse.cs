// <copyright file="LeaderboardSummaryListResponse.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Api.Clans
{
    using System.Collections.Generic;

    public class LeaderboardSummaryListResponse
    {
        public PaginationMetadata Pagination { get; set; }
        public IList<LeaderboardClan> LeaderboardClans { get; set; }
    }
}
