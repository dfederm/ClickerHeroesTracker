// <copyright file="Upload.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Api.Uploads
{
    using System;

    /// <summary>
    /// The details about a specific upload
    /// </summary>
    public sealed class Upload
    {
        public int Id { get; set; }

        public User User { get; set; }

        public DateTime TimeSubmitted { get; set; }

        public string Content { get; set; }

        public PlayStyle PlayStyle { get; set; }

        public bool IsScrubbed { get; set; }
    }
}