// <copyright file="ICounterProvider.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Instrumentation
{
    using System;

    public interface ICounterProvider
    {
        void Start(Counter counter);

        void Stop(Counter counter);

        IDisposable Measure(Counter counter);

        IDisposable Suspend(Counter counter);
    }
}
