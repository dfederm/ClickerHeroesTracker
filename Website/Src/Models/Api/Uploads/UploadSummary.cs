// <copyright file="UploadSummary.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Api.Uploads
{
    using System;

    /// <summary>
    /// The details about a specific upload
    /// </summary>
    public sealed class UploadSummary
    {
        public int Id { get; set; }

        public DateTime TimeSubmitted { get; set; }

        public DateTime SaveTime { get; set; }

        public int AscensionNumber { get; set; }

        public int Zone { get; set; }

        public string Souls { get; set; }
    }
}