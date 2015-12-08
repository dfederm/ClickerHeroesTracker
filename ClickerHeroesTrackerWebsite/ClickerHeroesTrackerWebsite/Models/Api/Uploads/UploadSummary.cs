// <copyright file="UploadSummary.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Api.Uploads
{
    using System;

    /// <summary>
    /// Summary data about an upload.
    /// </summary>
    public sealed class UploadSummary
    {
        /// <summary>
        /// Gets or sets the upload id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the time the upload was submitted.
        /// </summary>
        public DateTime TimeSubmitted { get; set; }
    }
}