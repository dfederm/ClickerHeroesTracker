// <copyright file="NoOpUploadScheduler.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Services.UploadProcessing
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

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
        public Task<IDictionary<string, int>> RetrieveQueueStatsAsync()
        {
            return Task.FromResult((IDictionary<string, int>)new Dictionary<string, int>());
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
