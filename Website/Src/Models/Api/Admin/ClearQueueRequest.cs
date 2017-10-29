// <copyright file="ClearQueueRequest.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace Website.Models.Api.Admin
{
    using ClickerHeroesTrackerWebsite.Services.UploadProcessing;

    public sealed class ClearQueueRequest
    {
        public UploadProcessingMessagePriority Priority { get; set; }
    }
}
