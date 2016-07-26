// <copyright file="Clan.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Api.Clans
{
    using System.Collections.Generic;

    public sealed class Clan
    {
        public Guild Guild { get; set; }
        public IDictionary<string, GuildMember> GuildMembers { get; set; }
        public User User { get; set; }
        public IList<Message> Messages { get; set; } 
    }
}
