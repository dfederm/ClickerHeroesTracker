// <copyright file="Series.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Dashboard.Graph
{
    using System.Collections.Generic;

    /// <summary>
    /// The data for a series on the graph.
    /// </summary>
    public class Series
    {
        /// <summary>
        /// Gets or sets the name of the series as shown in the legend, tooltip etc.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a list of data points for the series.
        /// </summary>
        public IList<Point> Data { get; set; }

        /// <summary>
        /// Gets or sets the main color of the series.
        /// </summary>
        public string Color { get; set; }
    }
}