// <copyright file="GraphRangeSelectorViewModel.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Dashboard.Graph
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlTypes;
    using System.Linq;

    /// <summary>
    /// The model for the graph range selector view
    /// </summary>
    public sealed class GraphRangeSelectorViewModel
    {
        private static string[] ranges =
        {
            "1d",
            "3d",
            "1w",
            "1m",
            "all",
        };

        internal GraphRangeSelectorViewModel(string range)
        {
            this.CurrentRange = range != null && GraphRangeSelectorViewModel.Ranges.Contains(range, StringComparer.OrdinalIgnoreCase)
                ? range.ToLowerInvariant()
                : GraphRangeSelectorViewModel.DefaultRange;

            this.End = DateTime.UtcNow;
            switch (this.CurrentRange)
            {
                case "1d":
                {
                    this.Start = this.End.AddDays(-1);
                    break;
                }

                case "3d":
                {
                    this.Start = this.End.AddDays(-3);
                    break;
                }

                case "1w":
                {
                    this.Start = this.End.AddDays(-7);
                    break;
                }

                case "1m":
                {
                    this.Start = this.End.AddMonths(-1);
                    break;
                }

                case "all":
                {
                    this.Start = SqlDateTime.MinValue.Value;
                    break;
                }
            }
        }

        /// <summary>
        /// Gets the possbile set of ranges
        /// </summary>
        public static IList<string> Ranges { get; } = ranges;

        /// <summary>
        /// Gets the default range when one is not specified
        /// </summary>
        public static string DefaultRange { get; } = "1w";

        /// <summary>
        /// Gets or sets the current range
        /// </summary>
        public string CurrentRange { get; set; }

        /// <summary>
        /// Gets the start time for the range
        /// </summary>
        public DateTime Start { get; }

        /// <summary>
        /// Gets the end time for the range
        /// </summary>
        public DateTime End { get; }
    }
}