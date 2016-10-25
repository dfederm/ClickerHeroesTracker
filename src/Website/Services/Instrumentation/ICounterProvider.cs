// <copyright file="ICounterProvider.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Instrumentation
{
    using System;

    /// <summary>
    /// Allows interaction with counters for a request, such as measuring latency.
    /// </summary>
    public interface ICounterProvider
    {
        /// <summary>
        /// Starts or resumes a counter.
        /// </summary>
        /// <param name="counter">The counter type</param>
        void Start(Counter counter);

        /// <summary>
        /// Stops or pauses a counter
        /// </summary>
        /// <param name="counter">The counter type</param>
        void Stop(Counter counter);

        /// <summary>
        /// A scope in which to measure latency for a counter.
        /// </summary>
        /// <remarks>
        /// Upon calling this method, the counter is started. Once the returned <see cref="IDisposable"/> is disposed, the counter is stopped.
        /// </remarks>
        /// <param name="counter">The counter type</param>
        /// <returns>A disposable object which stops the counter when disposed</returns>
        IDisposable Measure(Counter counter);

        /// <summary>
        /// A scope in which to suspend latency measurement for a counter.
        /// </summary>
        /// <remarks>
        /// Upon calling this method, the counter is paused. Once the returned <see cref="IDisposable"/> is disposed, the counter is resumed.
        /// </remarks>
        /// <param name="counter">The counter type</param>
        /// <returns>A disposable object which resumes the counter when disposed</returns>
        IDisposable Suspend(Counter counter);
    }
}
