// <copyright file="ClanData.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Api.Clans
{
    using System.Collections.Generic;

    public sealed class ClanData
    {
        public string ClanName { get; set; }

        public int CurrentRaidLevel { get; set; }

        public IList<GuildMember> GuildMembers { get; set; }

        public IList<Message> Messages { get; set; }
    }
}
