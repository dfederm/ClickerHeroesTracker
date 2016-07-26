// <copyright file="Message.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Api.Clans
{
    using System;

    public class Message
    {
        public DateTime Date { get; set; }
        public string Username { get; set; }
        public string Content { get; set; }
    }
}
