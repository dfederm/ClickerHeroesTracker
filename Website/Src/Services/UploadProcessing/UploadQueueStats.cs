// <copyright file="UploadQueueStats.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace Website.Services.UploadProcessing
{
    using ClickerHeroesTrackerWebsite.Services.UploadProcessing;

    public sealed class UploadQueueStats
    {
        public UploadProcessingMessagePriority Priority { get; set; }

        public int NumMessages { get; set; }
    }
}
