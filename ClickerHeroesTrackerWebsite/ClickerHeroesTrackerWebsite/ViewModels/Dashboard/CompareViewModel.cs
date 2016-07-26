// <copyright file="CompareViewModel.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Dashboard
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ClickerHeroesTrackerWebsite.Models.Dashboard.Graph;
    using ClickerHeroesTrackerWebsite.Models.Game;
    using ClickerHeroesTrackerWebsite.Models.Settings;
    using ClickerHeroesTrackerWebsite.Services.Database;
    using ClickerHeroesTrackerWebsite.ViewModels.Dashboard.Graph;
    using Microsoft.ApplicationInsights;

    /// <summary>
    /// A model for the compare view.
    /// </summary>
    public class CompareViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompareViewModel"/> class.
        /// </summary>
        public CompareViewModel(
            GameData gameData,
            TelemetryClient telemetryClient,
            IDatabaseCommandFactory databaseCommandFactory,
            IUserSettings userSettings,
            string userId1,
            string userName1,
            string userId2,
            string userName2,
            string range)
        {
            this.UserName1 = userName1;
            this.UserName2 = userName2;

            this.RangeSelector = new GraphRangeSelectorViewModel(range);

            var userData1 = new ProgressData(
                gameData,
                telemetryClient,
                databaseCommandFactory,
                userId1,
                this.RangeSelector.Start,
                this.RangeSelector.End);
            var userData2 = new ProgressData(
                gameData,
                telemetryClient,
                databaseCommandFactory,
                userId2,
                this.RangeSelector.Start,
                this.RangeSelector.End);

            this.ProminentGraphs = new List<GraphViewModel>();

            // Suppress if it's all 0's since 1.0 doesn't support this stat yet.
            if (userData1.SoulsPerHourData.Any(datum => datum.Value > 0)
                && userData2.SoulsPerHourData.Any(datum => datum.Value > 0))
            {
                this.ProminentGraphs.Add(this.CreateGraph(
                    "soulsPerHourGraph",
                    "Souls/hr",
                    this.UserName1,
                    userData1.SoulsPerHourData,
                    this.UserName2,
                    userData2.SoulsPerHourData,
                    userSettings));
            }

            // Suppress if it's all 0's since 1.0 doesn't support this stat yet.
            if (userData1.OptimalLevelData.Any(datum => datum.Value > 0)
                && userData2.OptimalLevelData.Any(datum => datum.Value > 0))
            {
                this.ProminentGraphs.Add(this.CreateGraph(
                    "optimalLevelGraph",
                    "Optimal Level",
                    this.UserName1,
                    userData1.OptimalLevelData,
                    this.UserName2,
                    userData2.OptimalLevelData,
                    userSettings));
            }

            this.ProminentGraphs.Add(this.CreateGraph(
                "soulsSpentGraph",
                "Souls Spent",
                this.UserName1,
                userData1.SoulsSpentData,
                this.UserName2,
                userData2.SoulsSpentData,
                userSettings));
            this.ProminentGraphs.Add(this.CreateGraph(
                "titanDamageGraph",
                "Titan Damage",
                this.UserName1,
                userData1.TitanDamageData,
                this.UserName2,
                userData2.TitanDamageData,
                userSettings));

            this.SecondaryGraphs = userData1
                .AncientLevelData
                .Select(x => this.CreateGraph(
                    x.Key.Name + "Graph",
                    x.Key.Name,
                    this.UserName1,
                    x.Value,
                    this.UserName2,
                    userData2.AncientLevelData.SafeGet(x.Key),
                    userSettings))
                .ToList();

            this.IsValid = userData1.IsValid && userData2.IsValid;
        }

        /// <summary>
        /// Gets a value indicating whether the model is valid.
        /// </summary>
        public bool IsValid { get; }

        /// <summary>
        /// Gets the first user's name.
        /// </summary>
        public string UserName1 { get; }

        /// <summary>
        /// Gets the second user's name.
        /// </summary>
        public string UserName2 { get; }

        /// <summary>
        /// Gets a list of prominent graphs.
        /// </summary>
        public IList<GraphViewModel> ProminentGraphs { get; }

        /// <summary>
        /// Gets a list of secondary graphs.
        /// </summary>
        public IList<GraphViewModel> SecondaryGraphs { get; }

        /// <summary>
        /// Gets the graph range selector for this page
        /// </summary>
        public GraphRangeSelectorViewModel RangeSelector { get; }

        private GraphViewModel CreateGraph(
            string id,
            string title,
            string userName1,
            IDictionary<DateTime, double> userData1,
            string userName2,
            IDictionary<DateTime, double> userData2,
            IUserSettings userSettings)
        {
            var timeZone = userSettings.TimeZone;

            var series = new List<Series>();
            TryAddSeries(series, timeZone, userName1, userData1, Colors.PrimarySeriesColor);
            TryAddSeries(series, timeZone, userName2, userData2, Colors.OpposingSeriesColor);

            return new GraphViewModel
            {
                Id = id,
                Data = new GraphData
                {
                    Chart = new Chart
                    {
                        Type = ChartType.Line
                    },
                    Title = new Title
                    {
                        Text = title
                    },
                    XAxis = new Axis
                    {
                        TickInterval = 24 * 3600 * 1000, // one day
                        Type = AxisType.Datetime,
                        TickWidth = 0,
                        GridLineWidth = 1,
                        Labels = new Labels
                        {
                            Align = Align.Left,
                            X = 3,
                            Y = -3,
                            Format = "{value:%m/%d}"
                        }
                    },
                    YAxis = new Axis
                    {
                        Labels = new Labels
                        {
                            Align = Align.Left,
                            X = 3,
                            Y = 16,
                            Format = "{value:.,0f}"
                        },
                        ShowFirstLabel = false,
                        Type = GetYAxisType(userData1, userData2, userSettings),
                    },
                    Legend = new Legend
                    {
                        Enabled = false
                    },
                    Series = series
                }
            };
        }

        private static List<Series> TryAddSeries(
            List<Series> series,
            TimeZoneInfo timeZone,
            string name,
            IDictionary<DateTime, double> data,
            string color)
        {
            if (data != null && data.Count > 0)
            {
                series.Add(new Series
                {
                    Name = name,
                    Color = color,
                    Data = data
                        .Select(datum => new Point
                        {
                            X = datum.Key.ToJavascriptTime(timeZone),
                            Y = datum.Value
                        })
                        .Concat(new[]
                        {
                            new Point
                            {
                                X = DateTime.UtcNow.ToJavascriptTime(timeZone),
                                Y = data.Last().Value
                            }
                        })
                        .ToList()
                });
            }

            return series;
        }

        private static AxisType GetYAxisType(
            IDictionary<DateTime, double> userData1,
            IDictionary<DateTime, double> userData2,
            IUserSettings userSettings)
        {
            if (userSettings.UseLogarithmicGraphScale)
            {
                var minUserData1 = userData1 != null ? userData1.Values.Min() : 0;
                var minUserData2 = userData2 != null ? userData2.Values.Min() : 0;

                var maxUserData1 = userData1 != null ? userData1.Values.Max() : 0;
                var maxUserData2 = userData2 != null ? userData2.Values.Max() : 0;

                var hasZeroUser1 = userData1 != null ? userData1.Values.Any(datum => datum == 0) : false;
                var hasZeroUser2 = userData2 != null ? userData2.Values.Any(datum => datum == 0) : false;

                if (Math.Max(maxUserData1, maxUserData2) - Math.Min(minUserData1, minUserData2) > userSettings.LogarithmicGraphScaleThreshold
                    && !hasZeroUser1
                    && !hasZeroUser2)
                {
                    return AxisType.Logarithmic;
                }
            }

            return AxisType.Linear;
        }
    }
}