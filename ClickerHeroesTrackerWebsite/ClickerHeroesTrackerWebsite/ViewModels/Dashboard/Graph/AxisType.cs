// <copyright file="AxisType.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Dashboard.Graph
{
    /// <summary>
    /// The axis type options
    /// </summary>
    public enum AxisType
    {
        /// <summary>
        /// Plot the data linearly
        /// </summary>
        Linear,

        /// <summary>
        /// Plot the data logarithmically
        /// </summary>
        Logarithmic,

        /// <summary>
        /// Plot the data as dates. The data should be in milliseconds
        /// </summary>
        Datetime
    }
}