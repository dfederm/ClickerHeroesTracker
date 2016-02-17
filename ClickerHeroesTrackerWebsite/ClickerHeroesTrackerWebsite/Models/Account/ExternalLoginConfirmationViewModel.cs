// <copyright file="ExternalLoginConfirmationViewModel.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Account
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// The model for the external login confirmation view.
    /// </summary>
    public class ExternalLoginConfirmationViewModel
    {
        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 5)]
        [RegularExpression("\\w+", ErrorMessage = "The {0} must only contain letters and numbers")]
        [Display(Name = "Username")]
        public string UserName { get; set; }
    }
}
