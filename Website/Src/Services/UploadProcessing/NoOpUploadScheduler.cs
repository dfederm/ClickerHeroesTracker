// <copyright file="NoOpUploadScheduler.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Services.UploadProcessing
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Website.Services.UploadProcessing;

    /// <summary>
    /// Upload scheduler which does a no-op.
    /// </summary>
    public class NoOpUploadScheduler : IUploadScheduler
    {
        /// <inheritdoc />
        public Task<int> ClearQueueAsync(UploadProcessingMessagePriority priority)
        {
            return Task.FromResult(0);
        }

        /// <inheritdoc />
        public Task<IEnumerable<UploadQueueStats>> RetrieveQueueStatsAsync()
        {
            return Task.FromResult(Enumerable.Empty<UploadQueueStats>());
        }

        /// <inheritdoc />
        public Task ScheduleAsync(UploadProcessingMessage message)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task ScheduleAsync(IEnumerable<UploadProcessingMessage> messages)
        {
            return Task.CompletedTask;
        }
    }
}
