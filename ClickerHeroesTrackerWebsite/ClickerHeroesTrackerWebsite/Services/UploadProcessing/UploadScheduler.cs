// <copyright file="UploadScheduler.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.UploadProcessing
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Instrumentation;
    using Microsoft.Extensions.Configuration;
    using Microsoft.ServiceBus.Messaging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <inheritdoc />
    public sealed class UploadScheduler : IUploadScheduler
    {
        private static readonly JsonSerializer Serializer = CreateSerializer();

        private readonly ICounterProvider counterProvider;

        private readonly Dictionary<UploadProcessingMessagePriority, QueueClient> clients;

        /// <summary>
        /// Initializes a new instance of the <see cref="UploadScheduler"/> class.
        /// </summary>
        public UploadScheduler(
            ICounterProvider counterProvider,
            IConfiguration configuration)
        {
            this.counterProvider = counterProvider;

            var connectionString = configuration["UploadProcessing:ConnectionString"];
            this.clients = new Dictionary<UploadProcessingMessagePriority, QueueClient>
            {
                { UploadProcessingMessagePriority.Low, QueueClient.CreateFromConnectionString(connectionString, "UploadProcessing-LowPriority") },
                { UploadProcessingMessagePriority.High, QueueClient.CreateFromConnectionString(connectionString, "UploadProcessing-HighPriority") },
            };
        }

        /// <inheritdoc />
        public async Task Schedule(UploadProcessingMessage message)
        {
            using (this.counterProvider.Measure(Counter.ScheduleUpload))
            {
                var client = this.clients[message.Priority];
                var brokeredMessage = CreateBrokeredMessage(message);
                await client.SendAsync(brokeredMessage);
            }
        }

        /// <inheritdoc />
        public async Task Schedule(IEnumerable<UploadProcessingMessage> messages)
        {
            using (this.counterProvider.Measure(Counter.BatchScheduleUpload))
            {
                foreach (var pair in this.clients)
                {
                    var priority = pair.Key;
                    var client = pair.Value;

                    var brokeredMessages = messages
                        .Where(message => message.Priority == priority)
                        .Select(CreateBrokeredMessage);
                    var batch = new List<BrokeredMessage>();
                    long batchSize = 0;

                    foreach (var brokeredMessage in brokeredMessages)
                    {
                        const long MaxBatchSizeInBytes = 64 * 1024; // 64 kb
                        if ((batchSize + brokeredMessage.Size) > MaxBatchSizeInBytes)
                        {
                            // Send current batch
                            await client.SendBatchAsync(batch);

                            // Initialize a new batch
                            batch.Clear();
                            batchSize = 0;
                        }

                        // Add to the current batch
                        batch.Add(brokeredMessage);
                        batchSize += brokeredMessage.Size;
                    }

                    // The final batch
                    if (batch.Count > 0)
                    {
                        await client.SendBatchAsync(batch);
                    }
                }
            }
        }

        private static BrokeredMessage CreateBrokeredMessage(UploadProcessingMessage message)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            Serializer.Serialize(writer, message);
            writer.Flush();
            stream.Position = 0;
            return new BrokeredMessage(stream);
        }

        private static JsonSerializer CreateSerializer()
        {
            var settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;
            settings.Converters.Add(new StringEnumConverter { CamelCaseText = true });

            return JsonSerializer.CreateDefault(settings);
        }
    }
}