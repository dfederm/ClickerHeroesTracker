// <copyright file="EmailSenderSettings.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Services.Email
{
    using System.Collections.Generic;

    public sealed class EmailSenderSettings
    {
        public string ApiKey { get; set; }

        public List<string> FeedbackRecievers { get; set; }
    }
}
