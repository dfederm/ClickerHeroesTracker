// <copyright file="GraphViewModel.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Dashboard.Graph
{
    /// <summary>
    /// The model for the graph view
    /// </summary>
    public class GraphViewModel
    {
        /// <summary>
        /// Gets or sets the html id for the graph element.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the graph data and configuration.
        /// </summary>
        public GraphData Data { get; set; }
    }
}