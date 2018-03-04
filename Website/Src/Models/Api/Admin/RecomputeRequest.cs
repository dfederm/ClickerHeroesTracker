// <copyright file="RecomputeRequest.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace Website.Models.Api.Admin
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using ClickerHeroesTrackerWebsite.Services.UploadProcessing;

    public sealed class RecomputeRequest
    {
        [Required]
        [MinLength(1)]
        public IList<int> UploadIds { get; set; }

        [Required]
        public UploadProcessingMessagePriority Priority { get; set; }
    }
}
