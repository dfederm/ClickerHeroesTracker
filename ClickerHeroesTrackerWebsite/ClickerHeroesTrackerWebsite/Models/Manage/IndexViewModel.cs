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

    public class IndexViewModel
    {
        public static IEnumerable<TimeZoneSelectItem> TimeZones { get; } = TimeZoneInfo
            .GetSystemTimeZones()
            .Select(tz => new TimeZoneSelectItem { Id = tz.Id, Name = tz.DisplayName });

        public bool HasPassword { get; set; }

        public IList<UserLoginInfo> Logins { get; set; }

        [Display(Name = "Time Zone")]
        public string TimeZoneId { get; set; }

        [Display(Name = "Public uploads")]
        public bool AreUploadsPublic { get; set; }

        [Display(Name = "Solomon formula")]
        public string SolomonFormula { get; set; }

        [Display(Name = "Play Style")]
        public string PlayStyle { get; set; }

        public class TimeZoneSelectItem
        {
            public string Id { get; set; }

            public string Name { get; set; }
        }
    }
}