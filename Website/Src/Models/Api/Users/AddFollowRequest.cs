// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System.ComponentModel.DataAnnotations;

namespace Website.Models.Api.Users
{
    public sealed class AddFollowRequest
    {
        [Required]
        [MinLength(1)]
        public string FollowUserName { get; set; }
    }
}
