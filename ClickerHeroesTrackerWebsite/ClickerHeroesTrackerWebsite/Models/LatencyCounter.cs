// <copyright file="LatencyCounter.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models
{
    using System.Diagnostics;
    using System.Web;

    public class LatencyCounter
    {
        private readonly string name;

        private readonly Stopwatch stopwatch;

        private bool isRecorded;

        private LatencyCounter(string name)
        {
            this.name = name;
            this.stopwatch = new Stopwatch();
        }

        public static LatencyCounter ActionLatency
        {
            get
            {
                return Get("Action");
            }
        }

        public static LatencyCounter ResultLatency
        {
            get
            {
                return Get("Result");
            }
        }

        public void Start()
        {
            this.stopwatch.Start();
        }

        public void Stop()
        {
            if (this.stopwatch.IsRunning)
            {
                this.stopwatch.Stop();
            }
        }

        public void Record()
        {
            // Only record once per request
            if (this.isRecorded)
            {
                return;
            }

            this.Stop();

            Telemetry.Client.TrackMetric("Latency_" + this.name, this.stopwatch.Elapsed.TotalMilliseconds);
            this.isRecorded = true;
        }

        private static LatencyCounter Get(string name)
        {
            var cacheKey = "Latency_" + name;
            return HttpContext.Current.Items[cacheKey] as LatencyCounter
                ?? (LatencyCounter)(HttpContext.Current.Items[cacheKey] = new LatencyCounter(name));
        }
    }
}