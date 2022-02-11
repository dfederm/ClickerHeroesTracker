// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System.Collections.Generic;
using Azure;
using Azure.Data.Tables;
using Microsoft.ApplicationInsights;

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

        public static void TrackFailedTableResult(this TelemetryClient telemetryClient, Response response, ITableEntity operationEntity)
        {
            Dictionary<string, string> properties = new()
            {
                { "OperationEntity-PartitionKey", operationEntity?.PartitionKey ?? "<none>" },
                { "OperationEntity-RowKey", operationEntity?.RowKey ?? "<none>" },
                { "Result-HttpStatusCode", response.Status.ToString() },
            };
            telemetryClient.TrackEvent("FailedTableResult", properties);
        }
    }
}