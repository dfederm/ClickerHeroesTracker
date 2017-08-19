// <copyright file="Point.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Dashboard.Graph
{
    using Newtonsoft.Json;

    /// <summary>
    /// A single point of data.
    /// </summary>
    [JsonConverter(typeof(PointConverter))]
    public class Point
    {
        /// <summary>
        /// Gets or sets the x value of the point. For datetime axes, the X value is the timestamp in milliseconds since 1970.
        /// </summary>
        public long X { get; set; }

        /// <summary>
        /// Gets or sets y value of the point.
        /// </summary>
        public string Y { get; set; }
    }
}