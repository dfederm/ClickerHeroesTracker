// <copyright file="AddFollowRequest.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace Website.Models.Api.Users
{
    using System.ComponentModel.DataAnnotations;

    public sealed class AddFollowRequest
    {
        [Required]
        [MinLength(1)]
        public string FollowUserName { get; set; }
    }
}
