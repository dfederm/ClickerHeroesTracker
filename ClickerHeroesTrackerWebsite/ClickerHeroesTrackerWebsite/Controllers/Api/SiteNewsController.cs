// <copyright file="SiteNewsController.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Controllers.Api
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;
    using Database;
    using Instrumentation;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNet.Identity;
    using Microsoft.WindowsAzure.Storage.Table;
    using Models.Api;
    using Models.Api.SiteNews;
    using Models.Api.Uploads;
    using Models.Calculator;
    using Models.Game;
    using Models.Home;
    using Models.SaveData;
    using Models.Settings;
    using Utility;

    /// <summary>
    /// This controller handles the set of APIs that manage site news
    /// </summary>
    [RoutePrefix("api/news")]
    public sealed class SiteNewsController : ApiController
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
        public HttpResponseMessage List()
        {
            var table = this.tableClient.GetTableReference("SiteNews");
            table.CreateIfNotExists();

            var query = new TableQuery<SiteNewsTableEntity>();

            // Group entities by date and sort by order in each group
            int rawr = 0;
            var entitiesByDate = new SortedDictionary<DateTime, SortedList<int, string>>();
            foreach (var entity in table.ExecuteQuery(query))
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

            return this.Request.CreateResponse(model);
        }

        /// <summary>
        /// Post a news entity
        /// </summary>
        /// <param name="entry">The news entry</param>
        /// <returns>A status code representing the result</returns>
        [Route("")]
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public HttpResponseMessage Post(SiteNewsEntry entry)
        {
            if (entry == null
                || entry.Messages == null
                || entry.Messages.Count == 0)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            var table = this.tableClient.GetTableReference("SiteNews");
            table.CreateIfNotExists();

            // Delete all rows in the partition first
            var query = new TableQuery<SiteNewsTableEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, entry.Date.ToString("yyyy-MM-dd")));
            var batchOperation = new TableBatchOperation();
            foreach (var entity in table.ExecuteQuery(query))
            {
                batchOperation.Delete(entity);
            }

            IList<TableResult> results;
            if (batchOperation.Count > 0)
            {
                results = table.ExecuteBatch(batchOperation);
                batchOperation = new TableBatchOperation();
            }

            // Next insert all messages
            for (int i = 0; i < entry.Messages.Count; i++)
            {
                batchOperation.Insert(new SiteNewsTableEntity(entry.Date, i) { Message = entry.Messages[i] });
            }

            results = table.ExecuteBatch(batchOperation);
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

            return this.Request.CreateResponse(returnStatusCode);
        }

        /// <summary>
        /// Delete a news entity
        /// </summary>
        /// <param name="date">The date to delete news entries from</param>
        /// <returns>A status code representing the result</returns>
        [Route("{date}")]
        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public HttpResponseMessage Delete(DateTime date)
        {
            var table = this.tableClient.GetTableReference("SiteNews");
            table.CreateIfNotExists();

            var query = new TableQuery<SiteNewsTableEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, date.ToString("yyyy-MM-dd")));
            var batchOperation = new TableBatchOperation();
            foreach (var entity in table.ExecuteQuery(query))
            {
                batchOperation.Delete(entity);
            }

            var results = table.ExecuteBatch(batchOperation);
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

            return this.Request.CreateResponse(returnStatusCode);
        }
    }
}