// <copyright file="UploadSummary.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Api.Uploads
{
    using System;

    public sealed class UploadSummary
    {
        public int Id { get; set; }

        public DateTime TimeSubmitted { get; set; }
    }
}