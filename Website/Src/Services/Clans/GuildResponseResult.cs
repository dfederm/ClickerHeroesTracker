// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System.Collections.Generic;
using ClickerHeroesTrackerWebsite.Models.Api.Clans;

namespace Website.Services.Clans
{
    public sealed class GuildResponseResult
    {
        public Guild Guild { get; set; }

        public IDictionary<string, GuildMember> GuildMembers { get; set; }
    }
}
