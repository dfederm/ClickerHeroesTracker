// <copyright file="Labels.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Dashboard.Graph
{
    /// <summary>
    /// Axis labels show the number or category for each tick.
    /// </summary>
    public class Labels
    {
        /// <summary>
        /// Gets or sets what part of the string the given position is anchored to.
        /// </summary>
        public Align? Align { get; set; }

        /// <summary>
        /// Gets or sets the x position offset of the label relative to the tick position on the axis.
        /// </summary>
        public int? X { get; set; }

        /// <summary>
        /// Gets or sets the y position offset of the label relative to the tick position on the axis. Null makes it adapt to the font size on bottom axis.
        /// </summary>
        public int? Y { get; set; }

        /// <summary>
        /// Gets or sets a format string for the axis label.
        /// </summary>
        /// <remarks>
        /// Formatting details: http://www.highcharts.com/docs/chart-concepts/labels-and-string-formatting
        /// </remarks>
        public string Format { get; set; }
    }
}