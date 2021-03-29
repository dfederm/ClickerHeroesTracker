// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System.ComponentModel.DataAnnotations;

namespace Website.Models.Api.Users
{
    public class ChangePasswordRequest
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 4)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }
    }
}
