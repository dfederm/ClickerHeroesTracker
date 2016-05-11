// <copyright file="UploadProcessingMessage.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Services.UploadProcessing
{
    /// <summary>
    /// Represents the data payload used to schedule and process uploads.
    /// </summary>
    public sealed class UploadProcessingMessage
    {
        /// <summary>
        /// Gets or sets the upload id to process.
        /// </summary>
        public int UploadId { get; set; }

        /// <summary>
        /// Gets or sets the user id of the user reuqesting the processing.
        /// </summary>
        public string Requester { get; set; }

        /// <summary>
        /// Gets or sets the priority for the processing request.
        /// </summary>
        public UploadProcessingMessagePriority Priority { get; set; }
    }
}