// <copyright file="RecomputeRequest.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace Website.Models.Api.Admin
{
    using System.Collections.Generic;
    using ClickerHeroesTrackerWebsite.Services.UploadProcessing;

    public sealed class RecomputeRequest
    {
        public IList<int> UploadIds { get; set; }

        public UploadProcessingMessagePriority Priority { get; set; }
    }
}
