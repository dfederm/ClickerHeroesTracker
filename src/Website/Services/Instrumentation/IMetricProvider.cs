// <copyright file="IMetricProvider.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Services.Instrumentation
{
    using ClickerHeroesTrackerWebsite.Instrumentation;
    using Microsoft.ApplicationInsights.Extensibility;

    public interface IMetricProvider
    {
        Metric GetMetric(Counter counter);
    }
}
