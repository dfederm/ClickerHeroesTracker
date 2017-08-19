// <copyright file="Counter.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Instrumentation
{
    /// <summary>
    /// Different kinds of counters the application tracks
    /// </summary>
    public enum Counter
    {
        /// <summary>
        /// Latency for an entire request
        /// </summary>
        Total,

        /// <summary>
        /// The internal latency (latency within the application)
        /// </summary>
        Internal,

        /// <summary>
        /// The latency waiting on external dependencies, like SQL.
        /// </summary>
        Dependency,

        /// <summary>
        /// The Mvc action latency
        /// </summary>
        Action,

        /// <summary>
        /// The Mvc result latency
        /// </summary>
        Result,

        /// <summary>
        /// The Upload Scheduling latency
        /// </summary>
        ScheduleUpload,

        /// <summary>
        /// The Batch Upload Scheduling latency
        /// </summary>
        BatchScheduleUpload,

        /// <summary>
        /// The Upload Processing latency
        /// </summary>
        ProcessUpload,
    }
}