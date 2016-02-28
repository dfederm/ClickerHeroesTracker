// <copyright file="IUploadScheduler.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.UploadProcessing
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Handles scheduling for upload processing
    /// </summary>
    public interface IUploadScheduler
    {
        /// <summary>
        /// Schedules an upload for processing
        /// </summary>
        /// <param name="message">The messages to send</param>
        /// <returns>The asynchronous operation</returns>
        Task Schedule(UploadProcessingMessage message);

        /// <summary>
        /// Schedules multiple uploads for processing
        /// </summary>
        /// <param name="messages">The upload ids to schedule processing for</param>
        /// <returns>The asynchronous operation</returns>
        Task Schedule(IEnumerable<UploadProcessingMessage> messages);
    }
}
