// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Website.Services.Telemetry;

namespace Website.Services.SiteNews
{
    public class AzureStorageSiteNewsProvider : ISiteNewsProvider
    {
        private readonly TableServiceClient _tableClient;

        private readonly ITelemetryClient _telemetryClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureStorageSiteNewsProvider"/> class.
        /// </summary>
        /// <param name="tableClient">The azure table client.</param>
        /// <param name="telemetryClient">The telemetry client to log errors.</param>
        public AzureStorageSiteNewsProvider(TableServiceClient tableClient, ITelemetryClient telemetryClient)
        {
            _tableClient = tableClient ?? throw new ArgumentNullException(nameof(tableClient));
            _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
        }

        /// <inheritdoc />
        public Task EnsureCreatedAsync() => GetTableClient().CreateIfNotExistsAsync();

        /// <inheritdoc />
        public async Task AddSiteNewsEntriesAsync(DateTime newsDate, IList<string> messages)
        {
            TableClient tableClient = GetTableClient();
            List<TableTransactionAction> batchOperation = new();

            string partitionKey = newsDate.ToString("yyyy-MM-dd");

            // Delete all rows in the partition first
            AsyncPageable<TableEntity> results = tableClient.QueryAsync<TableEntity>(filter: $"PartitionKey eq '{partitionKey}'");
            await foreach (TableEntity entity in results)
            {
                batchOperation.Add(new TableTransactionAction(TableTransactionActionType.Delete, entity));
            }

            if (batchOperation.Count > 0)
            {
                await tableClient.SubmitTransactionAsync(batchOperation);
                batchOperation.Clear();
            }

            // Next insert all messages
            for (int i = 0; i < messages.Count; i++)
            {
                var entity = new TableEntity(partitionKey, rowKey: i.ToString()) { { "Message", messages[i] } };
                batchOperation.Add(new TableTransactionAction(TableTransactionActionType.Add, entity));
            }

            Response<IReadOnlyList<Response>> responses = await tableClient.SubmitTransactionAsync(batchOperation);
            int? returnStatusCode = null;
            for (int i = 0; i < responses.Value.Count; i++)
            {
                Response response = responses.Value[i];
                if (response.IsError)
                {
                    TrackFailedTableResult(response, batchOperation[i].Entity);
                    returnStatusCode = response.Status;
                }
            }

            if (returnStatusCode.HasValue)
            {
                throw new InvalidOperationException("Failed to add news entries due to following http code: " + returnStatusCode.Value);
            }
        }

        /// <inheritdoc />
        public async Task DeleteSiteNewsForDateAsync(DateTime newsDate)
        {
            TableClient tableClient = GetTableClient();
            List<TableTransactionAction> batchOperation = new();

            string partitionKey = newsDate.ToString("yyyy-MM-dd");

            AsyncPageable<TableEntity> results = tableClient.QueryAsync<TableEntity>(filter: $"PartitionKey eq '{partitionKey}'");
            await foreach (TableEntity entity in results)
            {
                batchOperation.Add(new TableTransactionAction(TableTransactionActionType.Delete, entity));
            }

            Response<IReadOnlyList<Response>> responses = await tableClient.SubmitTransactionAsync(batchOperation);
            int? returnStatusCode = null;
            for (int i = 0; i < responses.Value.Count; i++)
            {
                Response response = responses.Value[i];
                if (response.IsError)
                {
                    TrackFailedTableResult(response, batchOperation[i].Entity);
                    returnStatusCode = response.Status;
                }
            }

            if (returnStatusCode.HasValue)
            {
                throw new InvalidOperationException("Failed to add news entries due to following http code: " + returnStatusCode.Value);
            }
        }

        /// <inheritdoc />
        public async Task<IDictionary<DateTime, IList<string>>> RetrieveSiteNewsEntriesAsync()
        {
            TableClient tableClient = GetTableClient();

            // Group entities by date and sort by order in each group
            int currentOrder = 0;
            SortedDictionary<DateTime, SortedList<int, string>> entitiesByDate = new();
            AsyncPageable<TableEntity> results = tableClient.QueryAsync<TableEntity>();
            await foreach (TableEntity entity in results)
            {
                if (!DateTime.TryParse(entity.PartitionKey, out DateTime date))
                {
                    _telemetryClient.TrackCustomEvent(
                        "InvalidTableEntry",
                        [
                            new KeyValuePair<string, string>("PartitionKey", entity.PartitionKey),
                            new KeyValuePair<string, string>("RowKey", entity.RowKey),
                        ]);
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

                entities.Add(order, entity.GetString("Message"));
            }

            // Select only the messages
            SortedDictionary<DateTime, IList<string>> entries = new();
            foreach (KeyValuePair<DateTime, SortedList<int, string>> entities in entitiesByDate)
            {
                entries.Add(entities.Key, entities.Value.Values);
            }

            return entries;
        }

        private TableClient GetTableClient() => _tableClient.GetTableClient("SiteNews");

        private void TrackFailedTableResult(Response response, ITableEntity tableEntity)
            => _telemetryClient.TrackCustomEvent(
                "FailedTableResult",
                [
                    new KeyValuePair<string, string>("OperationEntity-PartitionKey", tableEntity?.PartitionKey ?? "<none>"),
                    new KeyValuePair<string, string>("OperationEntity-RowKey", tableEntity?.RowKey ?? "<none>"),
                    new KeyValuePair<string, string>("Result-HttpStatusCode", response.Status.ToString()),
                ]);
    }
}
