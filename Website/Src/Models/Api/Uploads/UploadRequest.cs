// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System.ComponentModel.DataAnnotations;

namespace ClickerHeroesTrackerWebsite.Models.Api.Uploads
{
    /// <summary>
    /// Upload request.
    /// </summary>
    public sealed class UploadRequest
    {
        /// <summary>
        /// Gets or sets the encoded saved game data.
        /// </summary>
        [Required]
        public string EncodedSaveData { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to add the upload to the user's progress.
        /// </summary>
        public bool AddToProgress { get; set; }

        /// <summary>
        /// Gets or sets the playstyle for this upload.
        /// </summary>
        public PlayStyle? PlayStyle { get; set; }
    }
}