// <copyright file="RawUpload.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Api.Uploads
{
    /// <summary>
    /// Raw upload data
    /// </summary>
    public sealed class RawUpload
    {
        /// <summary>
        /// Gets or sets the encoded saved game data.
        /// </summary>
        public string EncodedSaveData { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to add the upload to the user's progress.
        /// </summary>
        public bool AddToProgress { get; set; }
    }
}