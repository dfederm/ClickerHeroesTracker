// <copyright file="GuildMember.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Api.Clans
{
    public sealed class GuildMember
    {
        public int HighestZone { get; set; }

        public string Nickname { get; set; }

        public string Uid { get; set; }
    }
}