// <copyright file="MetricProvider.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Services.Instrumentation
{
    using System;
    using System.Collections.Generic;
    using ClickerHeroesTrackerWebsite.Instrumentation;
    using Microsoft.ApplicationInsights.Extensibility;

    public sealed class MetricProvider : IMetricProvider
    {
        private Dictionary<Counter, Metric> metrics;

        public MetricProvider(MetricManager metricManager)
        {
            var counters = Enum.GetValues(typeof(Counter));
            this.metrics = new Dictionary<Counter, Metric>(counters.Length);
            for (var i = 0; i < counters.Length; i++)
            {
                var counter = (Counter)counters.GetValue(i);
                var metric = metricManager.CreateMetric("Latency_" + counter.ToString());
                this.metrics.Add(counter, metric);
            }
        }

        public Metric GetMetric(Counter counter)
        {
            return this.metrics[counter];
        }
    }
}
