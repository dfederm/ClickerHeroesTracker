// <copyright file="IUploadScheduler.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Services.UploadProcessing
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Website.Services.UploadProcessing;

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
        Task ScheduleAsync(UploadProcessingMessage message);

        /// <summary>
        /// Schedules multiple uploads for processing
        /// </summary>
        /// <param name="messages">The upload ids to schedule processing for</param>
        /// <returns>The asynchronous operation</returns>
        Task ScheduleAsync(IEnumerable<UploadProcessingMessage> messages);

        /// <summary>
        /// Clears the queue of message in the priority specified.
        /// </summary>
        /// <param name="priority">The priority to clear.</param>
        /// <returns>The number of items cleared.</returns>
        Task<int> ClearQueueAsync(UploadProcessingMessagePriority priority);

        /// <summary>
        /// Retrieves queue names and number of items in them.
        /// </summary>
        /// <returns>Mapping of queue name and item count.</returns>
        Task<IEnumerable<UploadQueueStats>> RetrieveQueueStatsAsync();
    }
}
