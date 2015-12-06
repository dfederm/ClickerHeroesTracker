// <copyright file="Point.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Dashboard.Graph
{
    using Newtonsoft.Json;

    [JsonConverter(typeof(PointConverter))]
    public class Point
    {
        public double X { get; set; }

        public double Y { get; set; }
    }
}