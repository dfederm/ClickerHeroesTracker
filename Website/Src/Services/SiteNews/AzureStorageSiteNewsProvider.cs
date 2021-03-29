// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using ClickerHeroesTrackerWebsite.Utility;
using Microsoft.ApplicationInsights;
using Microsoft.Azure.Cosmos.Table;

namespace Website.Services.SiteNews
{
    public class AzureStorageSiteNewsProvider : ISiteNewsProvider
    {
        private readonly CloudTableClient _tableClient;

        private readonly TelemetryClient _telemetryClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureStorageSiteNewsProvider"/> class.
        /// </summary>
        /// <param name="tableClient">The azure table client.</param>
        /// <param name="telemetryClient">The telemetry client to log errors.</param>
        public AzureStorageSiteNewsProvider(CloudTableClient tableClient, TelemetryClient telemetryClient)
        {
            _tableClient = tableClient ?? throw new ArgumentNullException(nameof(tableClient));
            _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
        }

        /// <inheritdoc />
        public Task EnsureCreatedAsync() => GetTableReference().CreateIfNotExistsAsync();

        /// <inheritdoc />
        public async Task AddSiteNewsEntriesAsync(DateTime newsDate, IList<string> messages)
        {
            CloudTable table = GetTableReference();

            // Delete all rows in the partition first
            TableQuery<SiteNewsTableEntity> query = new TableQuery<SiteNewsTableEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, newsDate.ToString("yyyy-MM-dd")));
            TableBatchOperation batchOperation = new();
            TableContinuationToken token = null;
            do
            {
                TableQuerySegment<SiteNewsTableEntity> segment = await table.ExecuteQuerySegmentedAsync(query, token);
                token = segment.ContinuationToken;
                foreach (SiteNewsTableEntity entity in segment)
                {
                    batchOperation.Delete(entity);
                }
            }
            while (token != null);

            if (batchOperation.Count > 0)
            {
                await table.ExecuteBatchAsync(batchOperation);
                batchOperation = new TableBatchOperation();
            }

            // Next insert all messages
            for (int i = 0; i < messages.Count; i++)
            {
                batchOperation.Insert(new SiteNewsTableEntity(newsDate, i) { Message = messages[i] });
            }

            TableBatchResult results = await table.ExecuteBatchAsync(batchOperation);
            HttpStatusCode returnStatusCode = HttpStatusCode.OK;
            foreach (TableResult result in results)
            {
                if (result.HttpStatusCode >= 400 && result.HttpStatusCode <= 499)
                {
                    _telemetryClient.TrackFailedTableResult(result);
                    returnStatusCode = HttpStatusCode.BadRequest;
                }

                if (result.HttpStatusCode >= 500 && result.HttpStatusCode <= 599)
                {
                    _telemetryClient.TrackFailedTableResult(result);
                    returnStatusCode = HttpStatusCode.InternalServerError;
                }
            }

            if (returnStatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException("Failed to add news entries due to following http code: " + returnStatusCode);
            }
        }

        /// <inheritdoc />
        public async Task DeleteSiteNewsForDateAsync(DateTime newsDate)
        {
            CloudTable table = GetTableReference();

            TableQuery<SiteNewsTableEntity> query = new TableQuery<SiteNewsTableEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, newsDate.ToString("yyyy-MM-dd")));
            TableBatchOperation batchOperation = new();
            TableContinuationToken token = null;
            do
            {
                TableQuerySegment<SiteNewsTableEntity> segment = await table.ExecuteQuerySegmentedAsync(query, token);
                token = segment.ContinuationToken;
                foreach (SiteNewsTableEntity entity in segment)
                {
                    batchOperation.Delete(entity);
                }
            }
            while (token != null);

            TableBatchResult results = await table.ExecuteBatchAsync(batchOperation);
            HttpStatusCode returnStatusCode = HttpStatusCode.OK;
            foreach (TableResult result in results)
            {
                if (result.HttpStatusCode >= 400 && result.HttpStatusCode <= 499)
                {
                    _telemetryClient.TrackFailedTableResult(result);
                    returnStatusCode = HttpStatusCode.BadRequest;
                }

                if (result.HttpStatusCode >= 500 && result.HttpStatusCode <= 599)
                {
                    _telemetryClient.TrackFailedTableResult(result);
                    returnStatusCode = HttpStatusCode.InternalServerError;
                }
            }

            if (returnStatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException("Failed to add news entries due to following http code: " + returnStatusCode);
            }
        }

        /// <inheritdoc />
        public async Task<IDictionary<DateTime, IList<string>>> RetrieveSiteNewsEntriesAsync()
        {
            CloudTable table = GetTableReference();

            TableQuery<SiteNewsTableEntity> query = new();

            // Group entities by date and sort by order in each group
            int currentOrder = 0;
            SortedDictionary<DateTime, SortedList<int, string>> entitiesByDate = new();
            TableContinuationToken token = null;
            do
            {
                TableQuerySegment<SiteNewsTableEntity> segment = await table.ExecuteQuerySegmentedAsync(query, token);
                token = segment.ContinuationToken;
                foreach (SiteNewsTableEntity entity in segment)
                {
                    if (!DateTime.TryParse(entity.PartitionKey, out DateTime date))
                    {
                        _telemetryClient.TrackInvalidTableEntry(entity);
                        continue;
                    }

                    if (!int.TryParse(entity.RowKey, out int order))
                    {
                        order = currentOrder++;
                    }

                    if (!entitiesByDate.TryGetValue(date, out SortedList<int, string> entities))
                    {
                        entities = new SortedList<int, string>();
                        entitiesByDate.Add(date, entities);
                    }

                    entities.Add(order, entity.Message);
                }
            }
            while (token != null);

            // Select only the messages
            SortedDictionary<DateTime, IList<string>> entries = new();
            foreach (KeyValuePair<DateTime, SortedList<int, string>> entities in entitiesByDate)
            {
                entries.Add(entities.Key, entities.Value.Values);
            }

            return entries;
        }

        private CloudTable GetTableReference() => _tableClient.GetTableReference("SiteNews");
    }
}
