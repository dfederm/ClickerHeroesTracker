// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System.ComponentModel.DataAnnotations;

namespace Website.Models.Api.Admin
{
    public sealed class BlockClanRequest
    {
        [Required]
        public string ClanName { get; set; }

        [Required]
        public bool IsBlocked { get; set; }
    }
}
