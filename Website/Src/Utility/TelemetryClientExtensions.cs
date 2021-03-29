// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System.Collections.Generic;
using Microsoft.ApplicationInsights;
using Microsoft.Azure.Cosmos.Table;

namespace ClickerHeroesTrackerWebsite.Utility
{
    internal static class TelemetryClientExtensions
    {
        public static void TrackInvalidTableEntry(this TelemetryClient telemetryClient, ITableEntity entity)
        {
            Dictionary<string, string> properties = new()
            {
                { "PartitionKey", entity.PartitionKey },
                { "RowKey", entity.RowKey },
            };
            telemetryClient.TrackEvent("InvalidTableEntry", properties);
        }

        public static void TrackFailedTableResult(this TelemetryClient telemetryClient, TableResult result, ITableEntity operationEntity = null)
        {
            ITableEntity resultEntity = result.Result as ITableEntity;
            Dictionary<string, string> properties = new()
            {
                { "OperationEntity-PartitionKey", operationEntity?.PartitionKey ?? "<none>" },
                { "OperationEntity-RowKey", operationEntity?.RowKey ?? "<none>" },
                { "Result-HttpStatusCode", result.HttpStatusCode.ToString() },
                { "ResultEntity-PartitionKey", resultEntity?.PartitionKey ?? "<none>" },
                { "ResultEntity-RowKey", resultEntity?.RowKey ?? "<none>" },
            };
            telemetryClient.TrackEvent("FailedTableResult", properties);
        }
    }
}