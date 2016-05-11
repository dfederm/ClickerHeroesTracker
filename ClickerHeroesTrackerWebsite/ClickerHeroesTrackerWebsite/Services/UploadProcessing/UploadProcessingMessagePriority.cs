// <copyright file="UploadProcessingMessagePriority.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Services.UploadProcessing
{
    /// <summary>
    /// Represents the priority of an <see cref="UploadProcessingMessage"/>
    /// </summary>
    public enum UploadProcessingMessagePriority
    {
        /// <summary>
        /// Low priority, generally used for background reprocessing of an upload.
        /// </summary>
        Low,

        /// <summary>
        /// High priority, generally used for user-initiated first-time processing of an upload.
        /// </summary>
        High,
    }
}