// <copyright file="SiteNewsController.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Controllers.Api
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using ClickerHeroesTrackerWebsite.Models.Api.SiteNews;
    using ClickerHeroesTrackerWebsite.Utility;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// This controller handles the set of APIs that manage site news
    /// </summary>
    [Route("api/news")]
    public sealed class SiteNewsController : Controller
    {
        private readonly TelemetryClient telemetryClient;

        private readonly CloudTableClient tableClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="SiteNewsController"/> class.
        /// </summary>
        public SiteNewsController(
            TelemetryClient telemetryClient,
            CloudTableClient tableClient)
        {
            this.telemetryClient = telemetryClient;
            this.tableClient = tableClient;
        }

        /// <summary>
        /// Gets the news entities
        /// </summary>
        /// <returns>A response with the schema <see cref="SiteNewsEntryListResponse"/></returns>
        [Route("")]
        [HttpGet]
        public async Task<IActionResult> List()
        {
            var table = this.tableClient.GetTableReference("SiteNews");
            await table.CreateIfNotExistsAsync();

            var query = new TableQuery<SiteNewsTableEntity>();

            // Group entities by date and sort by order in each group
            int rawr = 0;
            var entitiesByDate = new SortedDictionary<DateTime, SortedList<int, string>>();
            TableContinuationToken token = null;
            do
            {
                var segment = await table.ExecuteQuerySegmentedAsync(query, token);
                token = segment.ContinuationToken;
                foreach (var entity in segment)
                {
                    DateTime date;
                    int order;
                    if (!DateTime.TryParse(entity.PartitionKey, out date))
                    {
                        this.telemetryClient.TrackInvalidTableEntry(entity);
                        continue;
                    }

                    if (!int.TryParse(entity.RowKey, out order))
                    {
                        order = rawr++;
                    }

                    SortedList<int, string> entities;
                    if (!entitiesByDate.TryGetValue(date, out entities))
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

            var model = new SiteNewsEntryListResponse
            {
                Entries = entries
            };

            return this.Ok(model);
        }

        /// <summary>
        /// Post a news entity
        /// </summary>
        /// <param name="entry">The news entry</param>
        /// <returns>A status code representing the result</returns>
        [Route("")]
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Post(SiteNewsEntry entry)
        {
            if (entry == null
                || entry.Messages == null
                || entry.Messages.Count == 0)
            {
                return this.BadRequest();
            }

            var table = this.tableClient.GetTableReference("SiteNews");
            await table.CreateIfNotExistsAsync();

            // Delete all rows in the partition first
            var query = new TableQuery<SiteNewsTableEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, entry.Date.ToString("yyyy-MM-dd")));
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
            for (int i = 0; i < entry.Messages.Count; i++)
            {
                batchOperation.Insert(new SiteNewsTableEntity(entry.Date, i) { Message = entry.Messages[i] });
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

            return this.StatusCode((int)returnStatusCode);
        }

        /// <summary>
        /// Delete a news entity
        /// </summary>
        /// <param name="date">The date to delete news entries from</param>
        /// <returns>A status code representing the result</returns>
        [Route("{date}")]
        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(DateTime date)
        {
            var table = this.tableClient.GetTableReference("SiteNews");
            await table.CreateIfNotExistsAsync();

            var query = new TableQuery<SiteNewsTableEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, date.ToString("yyyy-MM-dd")));
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

            return this.StatusCode((int)returnStatusCode);
        }
    }
}