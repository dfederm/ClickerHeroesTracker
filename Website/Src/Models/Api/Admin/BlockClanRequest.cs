// <copyright file="BlockClanRequest.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace Website.Models.Api.Admin
{
    using System.ComponentModel.DataAnnotations;

    public sealed class BlockClanRequest
    {
        [Required]
        public string ClanName { get; set; }

        [Required]
        public bool IsBlocked { get; set; }
    }
}
