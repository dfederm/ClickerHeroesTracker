// <copyright file="AzureStorageSiteNewsProvider.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace Website.Services.SiteNews
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using ClickerHeroesTrackerWebsite.Utility;
    using Microsoft.ApplicationInsights;
    using Microsoft.WindowsAzure.Storage.Table;

    public class AzureStorageSiteNewsProvider : ISiteNewsProvider
    {
        private readonly CloudTableClient tableClient;

        private readonly TelemetryClient telemetryClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureStorageSiteNewsProvider"/> class.
        /// </summary>
        /// <param name="tableClient">The azure table client.</param>
        /// <param name="telemetryClient">The telemetry client to log errors.</param>
        public AzureStorageSiteNewsProvider(CloudTableClient tableClient, TelemetryClient telemetryClient)
        {
            this.tableClient = tableClient ?? throw new ArgumentNullException(nameof(tableClient));
            this.telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
        }

        /// <inheritdoc />
        public async Task AddSiteNewsEntriesAsync(DateTime newsDate, IList<string> messages)
        {
            var table = this.tableClient.GetTableReference("SiteNews");
            await table.CreateIfNotExistsAsync();

            // Delete all rows in the partition first
            var query = new TableQuery<SiteNewsTableEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, newsDate.ToString("yyyy-MM-dd")));
            var batchOperation = new TableBatchOperation();
            TableContinuationToken token = null;
            do
            {
                var segment = await table.ExecuteQuerySegmentedAsync(query, token);
                token = segment.ContinuationToken;
                foreach (var entity in segment)
                {
                    batchOperation.Delete(entity);
                }
            }
            while (token != null);

            IList<TableResult> results;
            if (batchOperation.Count > 0)
            {
                results = await table.ExecuteBatchAsync(batchOperation);
                batchOperation = new TableBatchOperation();
            }

            // Next insert all messages
            for (int i = 0; i < messages.Count; i++)
            {
                batchOperation.Insert(new SiteNewsTableEntity(newsDate, i) { Message = messages[i] });
            }

            results = await table.ExecuteBatchAsync(batchOperation);
            var returnStatusCode = HttpStatusCode.OK;
            foreach (var result in results)
            {
                if (result.HttpStatusCode >= 400 && result.HttpStatusCode <= 499)
                {
                    this.telemetryClient.TrackFailedTableResult(result);
                    returnStatusCode = HttpStatusCode.BadRequest;
                }

                if (result.HttpStatusCode >= 500 && result.HttpStatusCode <= 599)
                {
                    this.telemetryClient.TrackFailedTableResult(result);
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
            var table = this.tableClient.GetTableReference("SiteNews");
            await table.CreateIfNotExistsAsync();

            var query = new TableQuery<SiteNewsTableEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, newsDate.ToString("yyyy-MM-dd")));
            var batchOperation = new TableBatchOperation();
            TableContinuationToken token = null;
            do
            {
                var segment = await table.ExecuteQuerySegmentedAsync(query, token);
                token = segment.ContinuationToken;
                foreach (var entity in segment)
                {
                    batchOperation.Delete(entity);
                }
            }
            while (token != null);

            var results = await table.ExecuteBatchAsync(batchOperation);
            var returnStatusCode = HttpStatusCode.OK;
            foreach (var result in results)
            {
                if (result.HttpStatusCode >= 400 && result.HttpStatusCode <= 499)
                {
                    this.telemetryClient.TrackFailedTableResult(result);
                    returnStatusCode = HttpStatusCode.BadRequest;
                }

                if (result.HttpStatusCode >= 500 && result.HttpStatusCode <= 599)
                {
                    this.telemetryClient.TrackFailedTableResult(result);
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
            var table = this.tableClient.GetTableReference("SiteNews");
            await table.CreateIfNotExistsAsync();

            var query = new TableQuery<SiteNewsTableEntity>();

            // Group entities by date and sort by order in each group
            var currentOrder = 0;
            var entitiesByDate = new SortedDictionary<DateTime, SortedList<int, string>>();
            TableContinuationToken token = null;
            do
            {
                var segment = await table.ExecuteQuerySegmentedAsync(query, token);
                token = segment.ContinuationToken;
                foreach (var entity in segment)
                {
                    if (!DateTime.TryParse(entity.PartitionKey, out var date))
                    {
                        this.telemetryClient.TrackInvalidTableEntry(entity);
                        continue;
                    }

                    if (!int.TryParse(entity.RowKey, out var order))
                    {
                        order = currentOrder++;
                    }

                    if (!entitiesByDate.TryGetValue(date, out var entities))
                    {
                        entities = new SortedList<int, string>();
                        entitiesByDate.Add(date, entities);
                    }

                    entities.Add(order, entity.Message);
                }
            }
            while (token != null);

            // Select only the messages
            var entries = new SortedDictionary<DateTime, IList<string>>();
            foreach (var entities in entitiesByDate)
            {
                entries.Add(entities.Key, entities.Value.Values);
            }

            return entries;
        }
    }
}
