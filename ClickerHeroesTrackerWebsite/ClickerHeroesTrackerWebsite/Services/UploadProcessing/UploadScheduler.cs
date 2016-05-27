// <copyright file="UploadScheduler.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Services.UploadProcessing
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using ClickerHeroesTrackerWebsite.Instrumentation;
    using Microsoft.WindowsAzure.Storage.Queue;
    using Newtonsoft.Json;

    /// <inheritdoc />
    public sealed class UploadScheduler : IUploadScheduler
    {
        private readonly ICounterProvider counterProvider;

        private readonly Dictionary<UploadProcessingMessagePriority, CloudQueue> clients;

        /// <summary>
        /// Initializes a new instance of the <see cref="UploadScheduler"/> class.
        /// </summary>
        public UploadScheduler(
            ICounterProvider counterProvider,
            CloudQueueClient queueClient)
        {
            this.counterProvider = counterProvider;
            this.clients = new Dictionary<UploadProcessingMessagePriority, CloudQueue>
            {
                { UploadProcessingMessagePriority.Low, GetQueue(queueClient, UploadProcessingMessagePriority.Low) },
                { UploadProcessingMessagePriority.High, GetQueue(queueClient, UploadProcessingMessagePriority.High) },
            };
        }

        /// <inheritdoc />
        public async Task ScheduleAsync(UploadProcessingMessage message)
        {
            using (this.counterProvider.Measure(Counter.ScheduleUpload))
            {
                await this.ScheduleInternal(message);
            }
        }

        /// <inheritdoc />
        public async Task ScheduleAsync(IEnumerable<UploadProcessingMessage> messages)
        {
            using (this.counterProvider.Measure(Counter.BatchScheduleUpload))
            {
                // Use WaitAll to do them in parallel
                await Task.WhenAll(messages.Select(ScheduleInternal));
            }
        }

        private async Task ScheduleInternal(UploadProcessingMessage message)
        {
            var client = this.clients[message.Priority];
            var serializedMessage = JsonConvert.SerializeObject(message);
            var queueMessage = new CloudQueueMessage(serializedMessage);
            await client.AddMessageAsync(queueMessage);
        }

        private static CloudQueue GetQueue(CloudQueueClient queueClient, UploadProcessingMessagePriority priority)
        {
            var queue = queueClient.GetQueueReference($"upload-processing-{priority.ToString().ToLower()}-priority");
            queue.CreateIfNotExists();
            return queue;
        }
    }
}