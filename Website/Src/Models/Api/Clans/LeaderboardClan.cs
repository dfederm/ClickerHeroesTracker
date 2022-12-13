// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

namespace ClickerHeroesTrackerWebsite.Models.Api.Clans
{
    public sealed class LeaderboardClan
    {
        public string Name { get; set; }

        public int CurrentRaidLevel { get; set; }

        public int? CurrentNewRaidLevel { get; set; }

        public int MemberCount { get; set; }

        public int Rank { get; set; }

        public bool IsUserClan { get; set; }
    }
}
