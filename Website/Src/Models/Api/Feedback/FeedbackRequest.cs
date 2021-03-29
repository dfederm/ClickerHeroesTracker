// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System.ComponentModel.DataAnnotations;

namespace Website.Models.Api.Feedback
{
    /// <summary>
    /// Represents user-submitted feedback.
    /// </summary>
    public class FeedbackRequest
    {
        /// <summary>
        /// Gets or sets the user's feedback text.
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