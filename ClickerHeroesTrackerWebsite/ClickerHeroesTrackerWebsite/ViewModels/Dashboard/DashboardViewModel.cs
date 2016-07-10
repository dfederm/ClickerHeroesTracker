// <copyright file="DashboardViewModel.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Dashboard
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Security.Claims;
    using ClickerHeroesTrackerWebsite.Services.Database;
    using ClickerHeroesTrackerWebsite.Models.Dashboard.Graph;
    using ClickerHeroesTrackerWebsite.Models.Game;
    using ClickerHeroesTrackerWebsite.Models.Settings;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Identity;

    /// <summary>
    /// The model for the dashboard view.
    /// </summary>
    public class DashboardViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DashboardViewModel"/> class.
        /// </summary>
        public DashboardViewModel(
            GameData gameData,
            TelemetryClient telemetryClient,
            IDatabaseCommandFactory databaseCommandFactory,
            IUserSettingsProvider userSettingsProvider,
            ClaimsPrincipal user,
            UserManager<ApplicationUser> userManager)
        {
            var userId = userManager.GetUserId(user);

            var userSettings = userSettingsProvider.Get(userId);

            var startTime = DateTime.UtcNow.AddDays(-7);

            ProgressData data;
            var parameters = new Dictionary<string, object>
            {
                { "@UserId", userId },
                { "@StartTime", startTime },
            };
            using (var command = databaseCommandFactory.Create(
                "GetProgressData",
                CommandType.StoredProcedure,
                parameters))
            using (var reader = command.ExecuteReader())
            {
                data = new ProgressData(
                    gameData,
                    telemetryClient,
                    reader);
            }

            if (!data.IsValid || data.SoulsPerHourData.Count == 0)
            {
                return;
            }

            IDictionary<DateTime, double> dataSeries;
            string graphTitle;
            if (data.SoulsPerHourData.Any(datum => datum.Value > 0))
            {
                dataSeries = data.SoulsPerHourData;
                graphTitle = "Souls/hr";
            }
            else
            {
                dataSeries = data.SoulsSpentData;
                graphTitle = "Souls Spent";
            }

            this.ProgressSummaryGraph = new GraphData
            {
                Chart = new Chart
                {
                    Type = ChartType.Line
                },
                Title = new Title
                {
                    Text = graphTitle
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
                    Type = GetYAxisType(dataSeries, userSettings),
                },
                Legend = new Legend
                {
                    Enabled = false
                },
                Series = new Series[]
                {
                    new Series
                    {
                        Data = dataSeries
                            .Select(datum => new Point
                            {
                                X = datum.Key.ToJavascriptTime(userSettings.TimeZone),
                                Y = datum.Value
                            })
                            .Concat(new[]
                            {
                                new Point
                                {
                                    X = DateTime.UtcNow.ToJavascriptTime(userSettings.TimeZone),
                                    Y = dataSeries.Last().Value
                                }
                            })
                            .ToList()
                    }
                }
            };

            this.RivalDataList = new RivalDataList(databaseCommandFactory, userId);

            this.IsValid = true;
        }

        /// <summary>
        /// Gets a value indicating whether the model is valid
        /// </summary>
        public bool IsValid { get; }

        /// <summary>
        /// Gets the graph data for the progress summary
        /// </summary>
        public GraphData ProgressSummaryGraph { get; }

        /// <summary>
        /// Gets the rivals list data.
        /// </summary>
        public RivalDataList RivalDataList { get; }

        private static AxisType GetYAxisType(
            IDictionary<DateTime, double> data,
            IUserSettings userSettings)
        {
            return userSettings.UseLogarithmicGraphScale
                && data.Values.Max() - data.Values.Min() > userSettings.LogarithmicGraphScaleThreshold
                && !data.Values.Any(datum => datum == 0)
                ? AxisType.Logarithmic
                : AxisType.Linear;
        }
    }
}