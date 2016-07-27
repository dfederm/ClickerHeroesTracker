// <copyright file="User.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Api.Clans
{
    public sealed class User
    {
        public string GuildName { get; set; }
        public int HighestZone { get; set; }
        public bool? IsGuildRequest { get; set; }
        public string Nickname { get; set; }
        public string PasswordHash { get; set; }
        public string Uid { get; set; }
    }
}