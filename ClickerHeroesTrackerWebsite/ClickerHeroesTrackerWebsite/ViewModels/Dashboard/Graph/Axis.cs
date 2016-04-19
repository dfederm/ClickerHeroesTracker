// <copyright file="Axis.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Dashboard.Graph
{
    /// <summary>
    /// Graph axis data
    /// </summary>
    public class Axis
    {
        /// <summary>
        /// Gets or sets the tick interval
        /// </summary>
        public int? TickInterval { get; set; }

        /// <summary>
        /// Gets or sets the axis type
        /// </summary>
        public AxisType? Type { get; set; }

        /// <summary>
        /// Gets or sets the width of the ticks
        /// </summary>
        public int? TickWidth { get; set; }

        /// <summary>
        /// Gets or sets the width of the grid lines
        /// </summary>
        public int? GridLineWidth { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show the first label.
        /// </summary>
        public bool? ShowFirstLabel { get; set; }

        /// <summary>
        /// Gets or sets the label data
        /// </summary>
        public Labels Labels { get; set; }
    }
}