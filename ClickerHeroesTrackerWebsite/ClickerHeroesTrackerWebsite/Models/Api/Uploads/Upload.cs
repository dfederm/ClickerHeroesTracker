// <copyright file="Upload.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Api.Uploads
{
    using System;
    using System.Collections.Generic;
    using ClickerHeroesTrackerWebsite.Models.Api.Stats;

    /// <summary>
    /// The details about a specific upload
    /// </summary>
    public sealed class Upload
    {
        /// <summary>
        /// Gets or sets the upload id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the user who uploaded this upload.
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Gets or sets the time the upload was submitted.
        /// </summary>
        public DateTime TimeSubmitted { get; set; }

        /// <summary>
        /// Gets or sets the encoded upload data.
        /// </summary>
        public string UploadContent { get; set; }

        /// <summary>
        /// Gets or sets the play style for this upload
        /// </summary>
        public PlayStyle PlayStyle { get; set; }

        /// <summary>
        /// Gets or sets the upload stats.
        /// </summary>
        public IDictionary<StatType, double> Stats { get; set; }
    }
}