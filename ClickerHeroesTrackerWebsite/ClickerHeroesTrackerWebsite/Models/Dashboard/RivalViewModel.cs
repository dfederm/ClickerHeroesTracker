// <copyright file="RivalViewModel.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Dashboard
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Security.Principal;
    using Database;
    using Game;
    using Graph;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNet.Identity;
    using Settings;

    /// <summary>
    /// A model for the rival view.
    /// </summary>
    public class RivalViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RivalViewModel"/> class.
        /// </summary>
        public RivalViewModel(
            GameData gameData,
            TelemetryClient telemetryClient,
            IDatabaseCommandFactory databaseCommandFactory,
            IUserSettingsProvider userSettingsProvider,
            IPrincipal user,
            int rivalId,
            string range)
        {
            var userId = user.Identity.GetUserId();
            var userName = user.Identity.GetUserName();

            var userSettings = userSettingsProvider.Get(userId);

            this.RangeSelector = new GraphRangeSelectorViewModel(range);

            ProgressData userData;
            ProgressData rivalData;

            var parameters = new Dictionary<string, object>
            {
                { "@UserId", userId },
                { "@RivalId", rivalId },
                { "@StartTime", this.RangeSelector.Start },
                { "@EndTime", this.RangeSelector.End },
            };
            using (var command = databaseCommandFactory.Create(
                "GetRivalData",
                CommandType.StoredProcedure,
                parameters))
            using (var reader = command.ExecuteReader())
            {
                userData = new ProgressData(
                    gameData,
                    telemetryClient,
                    reader,
                    userSettings);
                if (!reader.NextResult())
                {
                    return;
                }

                if (reader.Read())
                {
                    this.RivalUserName = reader["RivalUserName"].ToString();
                }
                else
                {
                    return;
                }

                if (!reader.NextResult())
                {
                    return;
                }

                rivalData = new ProgressData(
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
                    userName,
                    userData.SoulsPerHourData,
                    this.RivalUserName,
                    rivalData.SoulsPerHourData,
                    userSettings.TimeZone),
                this.CreateGraph(
                    "optimalLevelGraph",
                    "Optimal Level",
                    userName,
                    userData.OptimalLevelData,
                    this.RivalUserName,
                    rivalData.OptimalLevelData,
                    userSettings.TimeZone),
                this.CreateGraph(
                    "soulsSpentGraph",
                    "Souls Spent",
                    userName,
                    userData.SoulsSpentData,
                    this.RivalUserName,
                    rivalData.SoulsSpentData,
                    userSettings.TimeZone),
                this.CreateGraph(
                    "titanDamageGraph",
                    "Titan Damage",
                    userName,
                    userData.TitanDamageData,
                    this.RivalUserName,
                    rivalData.TitanDamageData,
                    userSettings.TimeZone),
            };
            this.SecondaryGraphs = userData
                .AncientLevelData
                .Select(x => this.CreateGraph(
                    x.Key.Name + "Graph",
                    x.Key.Name,
                    userName,
                    x.Value,
                    this.RivalUserName,
                    rivalData.AncientLevelData.SafeGet(x.Key),
                    userSettings.TimeZone))
                .ToList();

            this.IsValid = !string.IsNullOrEmpty(this.RivalUserName)
                && userData.IsValid
                && rivalData.IsValid;
        }

        /// <summary>
        /// Gets a value indicating whether the model is valid.
        /// </summary>
        public bool IsValid { get; }

        /// <summary>
        /// Gets the rival's user name.
        /// </summary>
        public string RivalUserName { get; }

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
            string userName,
            IDictionary<DateTime, long> userData,
            string rivalName,
            IDictionary<DateTime, long> rivalData,
            TimeZoneInfo timeZone)
        {
            var series = new List<Series>();
            this.TryAddSeries(series, timeZone, userName, userData);
            this.TryAddSeries(series, timeZone, rivalName, rivalData);

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
                    Series = series
                }
            };
        }

        private List<Series> TryAddSeries(
            List<Series> series,
            TimeZoneInfo timeZone,
            string name,
            IDictionary<DateTime, long> data)
        {
            if (data != null && data.Count > 0)
            {
                series.Add(new Series
                {
                    Name = name,
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
    }
}