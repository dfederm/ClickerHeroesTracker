// <copyright file="PruneInvalidAuthTokensRequest.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace Website.Models.Api.Admin
{
    using System.ComponentModel.DataAnnotations;

    public class PruneInvalidAuthTokensRequest
    {
        [Range(1, 100000)]
        public int BatchSize { get; set; }
    }
}
