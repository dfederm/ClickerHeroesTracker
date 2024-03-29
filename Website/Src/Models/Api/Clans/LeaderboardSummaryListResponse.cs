﻿// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System.Collections.Generic;

namespace ClickerHeroesTrackerWebsite.Models.Api.Clans
{
    public class LeaderboardSummaryListResponse
    {
        public PaginationMetadata Pagination { get; set; }

        public IReadOnlyList<LeaderboardClan> LeaderboardClans { get; set; }
    }
}
