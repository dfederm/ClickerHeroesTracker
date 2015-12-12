// <copyright file="IUserSettings.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Settings
{
    using System;

    /// <summary>
    /// The user's persistent site settings
    /// </summary>
    public interface IUserSettings
    {
        /// <summary>
        /// Gets or sets the user's time zone
        /// </summary>
        TimeZoneInfo TimeZone { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user's uploads are public.
        /// </summary>
        bool AreUploadsPublic { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the reduced solomon formula (Log) as opposed to the more optimal one (Ln).
        /// </summary>
        bool UseReducedSolomonFormula { get; set; }

        /// <summary>
        /// Gets or sets the user's play style.
        /// </summary>
        PlayStyle PlayStyle { get; set; }
    }
}