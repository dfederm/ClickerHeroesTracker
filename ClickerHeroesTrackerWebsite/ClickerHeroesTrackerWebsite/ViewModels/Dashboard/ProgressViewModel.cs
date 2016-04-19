// <copyright file="ProgressViewModel.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Dashboard
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Security.Claims;
    using Database;
    using Game;
    using Graph;
    using Microsoft.ApplicationInsights;
    using Settings;

    /// <summary>
    /// Represents a user's progress data
    /// </summary>
    public class ProgressViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressViewModel"/> class.
        /// </summary>
        public ProgressViewModel(
            GameData gameData,
            TelemetryClient telemetryClient,
            IDatabaseCommandFactory databaseCommandFactory,
            IUserSettingsProvider userSettingsProvider,
            ClaimsPrincipal user,
            string range)
        {
            var userId = user.GetUserId();

            var userSettings = userSettingsProvider.Get(userId);

            this.RangeSelector = new GraphRangeSelectorViewModel(range);

            ProgressData data;

            var commandParameters = new Dictionary<string, object>
            {
                { "@UserId", userId },
                { "@StartTime", this.RangeSelector.Start },
                { "@EndTime", this.RangeSelector.End },
            };
            using (var command = databaseCommandFactory.Create(
                "GetProgressData",
                CommandType.StoredProcedure,
                commandParameters))
            using (var reader = command.ExecuteReader())
            {
                data = new ProgressData(
                    gameData,
                    telemetryClient,
                    reader,
                    userSettings);
            }

            this.ProminentGraphs = new List<GraphViewModel>
            {
                this.CreateGraph(
                    "soulsPerHourGraph",
                    "Souls/hr",
                    data.SoulsPerHourData,
                    userSettings.TimeZone),
                this.CreateGraph(
                    "optimalLevelGraph",
                    "Optimal Level",
                    data.OptimalLevelData,
                    userSettings.TimeZone),
                this.CreateGraph(
                    "soulsSpentGraph",
                    "Souls Spent",
                    data.SoulsSpentData,
                    userSettings.TimeZone),
                this.CreateGraph(
                    "titanDamageGraph",
                    "Titan Damage",
                    data.TitanDamageData,
                    userSettings.TimeZone),
            };
            this.SecondaryGraphs = data
                .AncientLevelData
                .Select(x => this.CreateGraph(
                    x.Key.Name + "Graph",
                    x.Key.Name,
                    x.Value,
                    userSettings.TimeZone))
                .ToList();

            this.IsValid = data.IsValid;
        }

        /// <summary>
        /// Gets the graph view models to display prominently
        /// </summary>
        public IList<GraphViewModel> ProminentGraphs { get; }

        /// <summary>
        /// Gets the graph view models to display secondary
        /// </summary>
        public IList<GraphViewModel> SecondaryGraphs { get; }

        /// <summary>
        /// Gets the graph range selector for this page
        /// </summary>
        public GraphRangeSelectorViewModel RangeSelector { get; }

        /// <summary>
        /// Gets a value indicating whether the model is valid
        /// </summary>
        public bool IsValid { get; }

        private GraphViewModel CreateGraph(
            string id,
            string title,
            IDictionary<DateTime, long> data,
            TimeZoneInfo timeZone)
        {
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
                        ShowFirstLabel = false
                    },
                    Legend = new Legend
                    {
                        Enabled = false
                    },
                    Series = new Series[]
                    {
                        new Series
                        {
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
                        }
                    }
                }
            };
        }
    }
}