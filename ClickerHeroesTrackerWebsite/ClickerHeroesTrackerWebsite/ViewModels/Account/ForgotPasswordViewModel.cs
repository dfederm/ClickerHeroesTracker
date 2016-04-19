// <copyright file="ForgotPasswordViewModel.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.ViewModels.Account
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// The model for the forgot password view
    /// </summary>
    public class ForgotPasswordViewModel
    {
        /// <summary>
        /// Gets or sets the email address for the account.
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
