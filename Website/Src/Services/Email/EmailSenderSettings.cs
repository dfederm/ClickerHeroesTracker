// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System.Collections.Generic;

namespace ClickerHeroesTrackerWebsite.Services.Email
{
    public sealed class EmailSenderSettings
    {
        public string ApiKey { get; set; }

        public List<string> FeedbackRecievers { get; set; }
    }
}
