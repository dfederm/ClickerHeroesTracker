// <copyright file="CounterProvider.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Instrumentation
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using ClickerHeroesTrackerWebsite.Utility;
    using Microsoft.ApplicationInsights;

    /// <summary>
    /// Allows interaction with counters for a request, such as measuring latency.
    /// </summary>
    public sealed class CounterProvider : DisposableBase, ICounterProvider
    {
        private static int totalCounters = Enum.GetValues(typeof(Counter)).Length;

        private readonly Dictionary<Counter, Stopwatch> counters = new Dictionary<Counter, Stopwatch>(totalCounters);

        private readonly TelemetryClient telemetryClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="CounterProvider"/> class.
        /// </summary>
        public CounterProvider(TelemetryClient telemetryClient)
        {
            this.telemetryClient = telemetryClient;
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
        protected override void Dispose(bool isDisposing)
        {
            foreach (var pair in this.counters)
            {
                var counterType = pair.Key;
                var counter = pair.Value;

                // Stop the counter just in case it's still running.
                counter.Stop();

                this.telemetryClient.TrackMetric("Latency_" + counterType.ToString(), counter.Elapsed.TotalMilliseconds);
            }
        }

        private Stopwatch Get(Counter counterType)
        {
            this.EnsureNotDisposed();

            Stopwatch counter;
            if (!this.counters.TryGetValue(counterType, out counter))
            {
                counter = new Stopwatch();
                this.counters.Add(counterType, counter);
            }

            return counter;
        }

        private sealed class MeasureScope : DisposableBase
        {
            private readonly Stopwatch counter;

            public MeasureScope(Stopwatch counter)
            {
                this.counter = counter;
                this.counter.Start();
            }

            protected override void Dispose(bool isDisposing)
            {
                this.counter.Stop();
            }
        }

        private sealed class SuspendScope : DisposableBase
        {
            private readonly Stopwatch counter;

            public SuspendScope(Stopwatch counter)
            {
                this.counter = counter;
                this.counter.Stop();
            }

            protected override void Dispose(bool isDisposing)
            {
                this.counter.Start();
            }
        }
    }
}