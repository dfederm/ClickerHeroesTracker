// <copyright file="TelemetryClientExtensions.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Utility
{
    using System.Collections.Generic;
    using Microsoft.ApplicationInsights;
    using Microsoft.WindowsAzure.Storage.Table;

    internal static class TelemetryClientExtensions
    {
        public static void TrackInvalidTableEntry(this TelemetryClient telemetryClient, ITableEntity entity)
        {
            var properties = new Dictionary<string, string>
            {
                { "PartitionKey", entity.PartitionKey },
                { "RowKey", entity.RowKey },
            };
            telemetryClient.TrackEvent("InvalidTableEntry", properties);
        }

        public static void TrackFailedTableResult(this TelemetryClient telemetryClient, TableResult result, ITableEntity operationEntity = null)
        {
            var resultEntity = result.Result as ITableEntity;
            var properties = new Dictionary<string, string>
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