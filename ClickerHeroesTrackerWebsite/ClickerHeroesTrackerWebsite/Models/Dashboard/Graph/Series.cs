// <copyright file="Series.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Dashboard.Graph
{
    using System.Collections.Generic;

    public class Series
    {
        public string Name { get; set; }

        public IList<Point> Data { get; set; }
    }
}