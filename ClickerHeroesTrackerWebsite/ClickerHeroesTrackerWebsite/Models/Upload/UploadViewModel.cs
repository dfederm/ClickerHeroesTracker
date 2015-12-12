// <copyright file="UploadViewModel.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Upload
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Model for the upload view.
    /// </summary>
    public class UploadViewModel
    {
        /// <summary>
        /// Gets or sets the encoded save data.
        /// </summary>
        [Required]
        [Display(Name = "Save Data")]
        public string EncodedSaveData { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to add the upload to the user's progress.
        /// </summary>
        [Display(Name = "Add this upload to my progress")]
        public bool AddToProgress { get; set; }
    }
}