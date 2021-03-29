// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System.ComponentModel.DataAnnotations;

namespace Website.Models.Api.Users
{
    public sealed class CreateUserRequest
    {
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 5)]
        [RegularExpression("\\w+", ErrorMessage = "The {0} must only contain letters and numbers")]
        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 4)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
    }
}
