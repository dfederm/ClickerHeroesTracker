// <copyright file="FeedbackRequest.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace Website.Models.Api.Feedback
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Represents user-submitted feedback
    /// </summary>
    public class FeedbackRequest
    {
        /// <summary>
        /// Gets or sets the user's feedback text
        /// </summary>
        [Required]
        [MinLength(1)]
        public string Comments { get; set; }

        /// <summary>
        /// Gets or sets the user's email address.
        /// </summary>
        /// <remarks>
        /// This will only be populated if the user is not logged in.
        /// </remarks>
        public string Email { get; set; }
    }
}