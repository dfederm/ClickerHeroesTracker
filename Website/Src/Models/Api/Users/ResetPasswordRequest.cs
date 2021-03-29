// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System.ComponentModel.DataAnnotations;

namespace Website.Models.Api.Users
{
    public class ResetPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
