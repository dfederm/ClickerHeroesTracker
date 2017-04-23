// <copyright file="Guild.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Api.Clans
{
    using System.Collections.Generic;

    public sealed class Guild
    {
        public int CurrentRaidLevel { get; set; }

        public string GuildMasterUid { get; set; }

        public IDictionary<string, MemberType> MemberUids { get; set; }

        public string Name { get; set; }
    }
}