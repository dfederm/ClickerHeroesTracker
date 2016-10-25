// <copyright file="ConsoleLoggingProcessor.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTracker.UploadProcessor
{
    using System;
    using System.Text;
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility;

    /// <summary>
    /// An <see cref="ITelemetryProcessor"/> which logs all telemetry to the console.
    /// </summary>
    public sealed class ConsoleLoggingProcessor : ITelemetryProcessor
    {
        private readonly ITelemetryProcessor next;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleLoggingProcessor"/> class.
        /// </summary>
        /// <param name="next">The next processor</param>
        public ConsoleLoggingProcessor(ITelemetryProcessor next)
        {
            this.next = next;
        }

        /// <inheritdoc />
        public void Process(ITelemetry item)
        {
            var sb = new StringBuilder();

            EventTelemetry eventTelemetry;
            ExceptionTelemetry exceptionTelemetry;
            MetricTelemetry metricTelemetry;

            if ((eventTelemetry = item as EventTelemetry) != null)
            {
                sb.Append(eventTelemetry.Name);
                foreach (var prop in eventTelemetry.Properties)
                {
                    sb.Append("; ");
                    sb.Append(prop.Key);
                    sb.Append("=");
                    sb.Append(prop.Value);
                }
            }
            else if ((exceptionTelemetry = item as ExceptionTelemetry) != null)
            {
                sb.Append(exceptionTelemetry.Exception.GetType().Name);
                sb.Append("; ");
                sb.Append(exceptionTelemetry.Exception.Message);
            }
            else if ((metricTelemetry = item as MetricTelemetry) != null)
            {
                sb.Append(metricTelemetry.Name);
                sb.Append(" = ");
                sb.Append(metricTelemetry.Value);
            }
            else if (item is DependencyTelemetry)
            {
                // NOOP, these are the SQL commands.
            }
            else if (item is PerformanceCounterTelemetry)
            {
                // NOOP, we don't really care about machine perf counters
            }
            else
            {
                sb.Append(item.GetType().Name);
                sb.Append("; Unknown Telemetry!!");
            }

            if (sb.Length > 0)
            {
                Console.WriteLine(sb.ToString());
            }

            this.next.Process(item);
        }
    }
}
