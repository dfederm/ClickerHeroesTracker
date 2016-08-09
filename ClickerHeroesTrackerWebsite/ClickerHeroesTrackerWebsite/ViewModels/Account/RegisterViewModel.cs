// <copyright file="RegisterViewModel.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.ViewModels.Account
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// The model for the register view
    /// </summary>
    public class RegisterViewModel
    {
        /// <summary>
        /// Gets or sets the user name to register
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 5)]
        [RegularExpression("\\w+", ErrorMessage = "The {0} must only contain letters and numbers")]
        [Display(Name = "Username")]
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the email address to register
        /// </summary>
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the password for the account
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 4)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets a confirmation of the password for the account.
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
