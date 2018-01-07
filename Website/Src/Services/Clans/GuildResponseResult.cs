// <copyright file="GuildResponseResult.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace Website.Services.Clans
{
    using System.Collections.Generic;
    using ClickerHeroesTrackerWebsite.Models.Api.Clans;

    public sealed class GuildResponseResult
    {
        public Guild Guild { get; set; }

        public IDictionary<string, GuildMember> GuildMembers { get; set; }
    }
}
