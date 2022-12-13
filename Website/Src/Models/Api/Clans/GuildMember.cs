// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

namespace ClickerHeroesTrackerWebsite.Models.Api.Clans
{
    public sealed class GuildMember
    {
        public int HighestZone { get; set; }

        public string Nickname { get; set; }

        public string Uid { get; set; }

        public ClanClassType? ChosenClass { get; set; }

        public int? ClassLevel { get; set; }

        // Note! This is not in the guild API's JSON response and only augmented in our API response.
        // TODO: Separate the mode from the API response
        public string UserName { get; set; }
    }
}