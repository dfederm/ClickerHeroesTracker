// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System.Collections.Generic;

namespace ClickerHeroesTrackerWebsite.Models.Api.Clans
{
    public sealed class ClanData
    {
        public string ClanName { get; set; }

        public int CurrentRaidLevel { get; set; }

        public IList<GuildMember> GuildMembers { get; set; }

        public int Rank { get; set; }

        public bool IsBlocked { get; set; }
    }
}
