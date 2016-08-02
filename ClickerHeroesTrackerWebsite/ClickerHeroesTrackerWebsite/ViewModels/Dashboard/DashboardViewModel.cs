// <copyright file="DashboardViewModel.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Dashboard
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using ClickerHeroesTrackerWebsite.Services.Database;
    using ClickerHeroesTrackerWebsite.Models.Dashboard.Graph;
    using ClickerHeroesTrackerWebsite.Models.Game;
    using ClickerHeroesTrackerWebsite.Models.Settings;
    using ClickerHeroesTrackerWebsite.ViewModels.Dashboard.Graph;
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

            var data = new ProgressData(
                gameData,
                telemetryClient,
                databaseCommandFactory,
                userId,
                startTime,
                null);

            if (!data.IsValid)
            {
                return;
            }

            var dataSeries = data.SoulsSpentData;
            if (dataSeries.Count > 0)
            {
                this.ProgressSummaryGraph = new GraphData
                {
                    Chart = new Chart
                    {
                        Type = ChartType.Line
                    },
                    Title = new Title
                    {
                        Text = "Souls Spent"
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
                            Color = Colors.PrimarySeriesColor,
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
            }

            this.Follows = new List<string>();
            var parameters = new Dictionary<string, object>
            {
                { "@UserId", userId }
            };
            const string GetUserFollowsCommandText = @"
	            SELECT UserName
	            FROM UserFollows
	            INNER JOIN AspNetUsers
	            ON UserFollows.FollowUserId = AspNetUsers.Id
	            WHERE UserId = @UserId
	            ORDER BY UserName ASC";
            using (var command = databaseCommandFactory.Create(
                GetUserFollowsCommandText,
                parameters))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    this.Follows.Add(reader["UserName"].ToString());
                }
            }

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
        /// Gets the names of the users the user follows.
        /// </summary>
        public IList<string> Follows { get; }

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