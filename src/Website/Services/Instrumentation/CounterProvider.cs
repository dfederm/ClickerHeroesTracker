// <copyright file="CounterProvider.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Instrumentation
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using ClickerHeroesTrackerWebsite.Services.Instrumentation;
    using Microsoft.ApplicationInsights;

    /// <summary>
    /// Allows interaction with counters for a request, such as measuring latency.
    /// </summary>
    public sealed class CounterProvider : IDisposable, ICounterProvider
    {
        private static int totalCounters = Enum.GetValues(typeof(Counter)).Length;

        private readonly Dictionary<Counter, Stopwatch> counters = new Dictionary<Counter, Stopwatch>(totalCounters);

        private readonly TelemetryClient telemetryClient;

        private readonly IMetricProvider metricProvider;

        public CounterProvider(TelemetryClient telemetryClient, IMetricProvider metricProvider)
        {
            this.telemetryClient = telemetryClient;
            this.metricProvider = metricProvider;
        }

        /// <inheritdoc/>
        public void Start(Counter counter)
        {
            this.Get(counter).Start();
        }

        /// <inheritdoc/>
        public void Stop(Counter counter)
        {
            this.Get(counter).Stop();
        }

        /// <inheritdoc/>
        public IDisposable Measure(Counter counter)
        {
            return new MeasureScope(this.Get(counter));
        }

        /// <inheritdoc/>
        public IDisposable Suspend(Counter counter)
        {
            return new SuspendScope(this.Get(counter));
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            foreach (var pair in this.counters)
            {
                var counterType = pair.Key;
                var counter = pair.Value;

                // Stop the counter just in case it's still running.
                counter.Stop();

                var metric = this.metricProvider.GetMetric(counterType);
                metric.Track(counter.Elapsed.TotalMilliseconds);
            }
        }

        private Stopwatch Get(Counter counterType)
        {
            Stopwatch counter;
            if (!this.counters.TryGetValue(counterType, out counter))
            {
                counter = new Stopwatch();
                this.counters.Add(counterType, counter);
            }

            return counter;
        }

        private sealed class MeasureScope : IDisposable
        {
            private readonly Stopwatch counter;

            public MeasureScope(Stopwatch counter)
            {
                this.counter = counter;
                this.counter.Start();
            }

            public void Dispose()
            {
                this.counter.Stop();
            }
        }

        private sealed class SuspendScope : IDisposable
        {
            private readonly Stopwatch counter;

            public SuspendScope(Stopwatch counter)
            {
                this.counter = counter;
                this.counter.Stop();
            }

            public void Dispose()
            {
                this.counter.Start();
            }
        }
    }
}