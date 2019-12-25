// <copyright file="AzureStorageUploadScheduler.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Services.UploadProcessing
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Storage.Queue;
    using Newtonsoft.Json;
    using Website.Services.UploadProcessing;

    /// <inheritdoc />
    public sealed class AzureStorageUploadScheduler : IUploadScheduler
    {
        private readonly Dictionary<UploadProcessingMessagePriority, CloudQueue> clients;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureStorageUploadScheduler"/> class.
        /// </summary>
        public AzureStorageUploadScheduler(CloudQueueClient queueClient)
        {
            this.clients = new Dictionary<UploadProcessingMessagePriority, CloudQueue>
            {
                { UploadProcessingMessagePriority.Low, GetQueue(queueClient, UploadProcessingMessagePriority.Low) },
                { UploadProcessingMessagePriority.High, GetQueue(queueClient, UploadProcessingMessagePriority.High) },
            };
        }

        /// <inheritdoc />
        public async Task ScheduleAsync(UploadProcessingMessage message)
        {
            var client = this.clients[message.Priority];
            var serializedMessage = JsonConvert.SerializeObject(message);
            var queueMessage = new CloudQueueMessage(serializedMessage);
            await client.AddMessageAsync(queueMessage);
        }

        /// <inheritdoc />
        public async Task ScheduleAsync(IEnumerable<UploadProcessingMessage> messages)
        {
            // Use WaitAll to do them in parallel
            await Task.WhenAll(messages.Select(this.ScheduleAsync));
        }

        /// <inheritdoc />
        public async Task<int> ClearQueueAsync(UploadProcessingMessagePriority priority)
        {
            var queue = this.clients[priority];
            var numMessages = queue.ApproximateMessageCount.GetValueOrDefault();

            await queue.ClearAsync();
            return numMessages;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<UploadQueueStats>> RetrieveQueueStatsAsync()
        {
            var queueStats = new List<UploadQueueStats>(this.clients.Count);
            foreach (var queue in this.clients)
            {
                await queue.Value.FetchAttributesAsync();
                var numMessages = queue.Value.ApproximateMessageCount.GetValueOrDefault();
                queueStats.Add(new UploadQueueStats { Priority = queue.Key, NumMessages = numMessages });
            }

            return queueStats;
        }

        private static CloudQueue GetQueue(CloudQueueClient queueClient, UploadProcessingMessagePriority priority)
        {
            var queue = queueClient.GetQueueReference($"upload-processing-{priority.ToString().ToLower(CultureInfo.InvariantCulture)}-priority");
            queue.CreateIfNotExistsAsync().Wait();
            return queue;
        }
    }
}