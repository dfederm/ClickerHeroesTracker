// <copyright file="IndexViewModel.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using Microsoft.AspNet.Identity;

    /// <summary>
    /// Model for the manage view.
    /// </summary>
    public class IndexViewModel
    {
        /// <summary>
        /// Gets a collection of all supported time zones.
        /// </summary>
        public static IEnumerable<TimeZoneSelectItem> TimeZones { get; } = TimeZoneInfo
            .GetSystemTimeZones()
            .Select(tz => new TimeZoneSelectItem { Id = tz.Id, Name = tz.DisplayName });

        /// <summary>
        /// Gets or sets a value indicating whether the user has a password set.
        /// </summary>
        public bool HasPassword { get; set; }

        /// <summary>
        /// Gets or sets a list of login providers the user has set up.
        /// </summary>
        public IList<UserLoginInfo> Logins { get; set; }

        /// <summary>
        /// Gets or sets the user's time zone id.
        /// </summary>
        [Display(Name = "Time Zone")]
        public string TimeZoneId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user's uploads are public.
        /// </summary>
        [Display(Name = "Public uploads")]
        public bool AreUploadsPublic { get; set; }

        /// <summary>
        /// Gets or sets the user's preferred solomon formula (Log or Ln).
        /// </summary>
        [Display(Name = "Solomon formula")]
        public string SolomonFormula { get; set; }

        /// <summary>
        /// Gets or sets the user's preferred play style.
        /// </summary>
        [Display(Name = "Play Style")]
        public string PlayStyle { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user wants to see experimental stats
        /// </summary>
        [Display(Name = "Experimental stats")]
        public bool UseExperimentalStats { get; set; }

        /// <summary>
        /// Model for an option for the time zone select control.
        /// </summary>
        public class TimeZoneSelectItem
        {
            /// <summary>
            /// Gets or sets the time zone id
            /// </summary>
            public string Id { get; set; }

            /// <summary>
            /// Gets or sets the time zone display name.
            /// </summary>
            public string Name { get; set; }
        }
    }
}