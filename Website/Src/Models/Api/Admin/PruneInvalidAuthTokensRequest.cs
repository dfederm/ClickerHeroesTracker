// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System.ComponentModel.DataAnnotations;

namespace Website.Models.Api.Admin
{
    public class PruneInvalidAuthTokensRequest
    {
        [Range(1, 100000)]
        public int BatchSize { get; set; }
    }
}
