// <copyright file="IUserSettings.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Settings
{
    using System;

    public interface IUserSettings
    {
        TimeZoneInfo TimeZone { get; set; }

        bool AreUploadsPublic { get; set; }

        bool UseReducedSolomonFormula { get; set; }

        PlayStyle PlayStyle { get; set; }
    }
}