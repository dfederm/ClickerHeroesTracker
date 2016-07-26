// <copyright file="MessageResponseContent.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Api.Clans
{
    using System.Collections.Generic;

    public class MessageResponseContent
    {
        public string GuildName { get; set; }
        public IDictionary<string, string> Messages { get; set; }
    }
}
