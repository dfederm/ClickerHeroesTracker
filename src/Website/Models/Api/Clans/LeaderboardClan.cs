// <copyright file="LeaderboardClan.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Api.Clans
{
    public sealed class LeaderboardClan
    {
        public string Name { get; set; }

        public int CurrentRaidLevel { get; set; }

        public int MemberCount { get; set; }

        public int Rank { get; set; }

        public bool IsUserClan { get; set; }
    }
}
