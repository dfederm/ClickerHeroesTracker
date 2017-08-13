// <copyright file="Chart.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Dashboard.Graph
{
    /// <summary>
    /// Options regarding the chart area and plot area as well as general chart options.
    /// </summary>
    public class Chart
    {
        /// <summary>
        /// Gets or sets the the default series type for the chart.
        /// </summary>
        public ChartType Type { get; set; }
    }
}