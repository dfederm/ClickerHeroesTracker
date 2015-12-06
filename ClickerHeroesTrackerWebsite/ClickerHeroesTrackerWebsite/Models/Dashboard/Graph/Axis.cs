// <copyright file="Axis.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Dashboard.Graph
{
    public class Axis
    {
        public int? TickInterval { get; set; }

        public AxisType? Type { get; set; }

        public int? TickWidth { get; set; }

        public int? GridLineWidth { get; set; }

        public bool? ShowFirstLabel { get; set; }

        public Labels Labels { get; set; }
    }
}