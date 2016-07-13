// <copyright file="RivalViewModel.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Dashboard
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Security.Claims;
    using ClickerHeroesTrackerWebsite.Models.Dashboard.Graph;
    using ClickerHeroesTrackerWebsite.Models.Game;
    using ClickerHeroesTrackerWebsite.Models.Settings;
    using ClickerHeroesTrackerWebsite.Services.Database;
    using ClickerHeroesTrackerWebsite.Utility;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Identity;

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
            ClaimsPrincipal principal,
            UserManager<ApplicationUser> userManager,
            int rivalId,
            string range)
        {
            var user = AsyncHelper.RunSynchronously(async () => await userManager.GetUserAsync(principal));
            var userId = user.Id;
            var userName = user.UserName;

            var userSettings = userSettingsProvider.Get(userId);

            this.RangeSelector = new GraphRangeSelectorViewModel(range);

            // BUGBUG 126 - Get the two users to compare from the query string.
            var parameters = new Dictionary<string, object>
            {
                { "@UserId", userId },
                { "@RivalId", rivalId },
            };
            const string GetRivalDataCommandText = @"
	            SELECT Id, UserName
	            FROM AspNetUsers
	            WHERE Id = (
		            SELECT RivalUserId
		            FROM Rivals
		            WHERE Id = @RivalId
		            AND UserId = @UserId
	            )";

            string rivalUserId;
            using (var command = databaseCommandFactory.Create(
                GetRivalDataCommandText,
                parameters))
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    rivalUserId = reader["Id"].ToString();
                    this.RivalUserName = reader["UserName"].ToString();
                }
                else
                {
                    return;
                }
            }

            var userData = new ProgressData(
                gameData,
                telemetryClient,
                databaseCommandFactory,
                userId,
                this.RangeSelector.Start,
                this.RangeSelector.End);
            var rivalData = new ProgressData(
                gameData,
                telemetryClient,
                databaseCommandFactory,
                rivalUserId,
                this.RangeSelector.Start,
                this.RangeSelector.End);

            this.ProminentGraphs = new List<GraphViewModel>();

            // Suppress if it's all 0's since 1.0 doesn't support this stat yet.
            if (userData.SoulsPerHourData.Any(datum => datum.Value > 0)
                && rivalData.SoulsPerHourData.Any(datum => datum.Value > 0))
            {
                this.ProminentGraphs.Add(this.CreateGraph(
                    "soulsPerHourGraph",
                    "Souls/hr",
                    userName,
                    userData.SoulsPerHourData,
                    this.RivalUserName,
                    rivalData.SoulsPerHourData,
                    userSettings));
            }

            // Suppress if it's all 0's since 1.0 doesn't support this stat yet.
            if (userData.OptimalLevelData.Any(datum => datum.Value > 0)
                && rivalData.OptimalLevelData.Any(datum => datum.Value > 0))
            {
                this.ProminentGraphs.Add(this.CreateGraph(
                    "optimalLevelGraph",
                    "Optimal Level",
                    userName,
                    userData.OptimalLevelData,
                    this.RivalUserName,
                    rivalData.OptimalLevelData,
                    userSettings));
            }

            this.ProminentGraphs.Add(this.CreateGraph(
                "soulsSpentGraph",
                "Souls Spent",
                userName,
                userData.SoulsSpentData,
                this.RivalUserName,
                rivalData.SoulsSpentData,
                userSettings));
            this.ProminentGraphs.Add(this.CreateGraph(
                "titanDamageGraph",
                "Titan Damage",
                userName,
                userData.TitanDamageData,
                this.RivalUserName,
                rivalData.TitanDamageData,
                userSettings));

            this.SecondaryGraphs = userData
                .AncientLevelData
                .Select(x => this.CreateGraph(
                    x.Key.Name + "Graph",
                    x.Key.Name,
                    userName,
                    x.Value,
                    this.RivalUserName,
                    rivalData.AncientLevelData.SafeGet(x.Key),
                    userSettings))
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
            IDictionary<DateTime, double> userData,
            string rivalName,
            IDictionary<DateTime, double> rivalData,
            IUserSettings userSettings)
        {
            var timeZone = userSettings.TimeZone;

            var series = new List<Series>();
            TryAddSeries(series, timeZone, userName, userData);
            TryAddSeries(series, timeZone, rivalName, rivalData);

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
                        Type = GetYAxisType(userData, rivalData, userSettings),
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
            IDictionary<DateTime, double> data)
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

        private static AxisType GetYAxisType(
            IDictionary<DateTime, double> userData,
            IDictionary<DateTime, double> rivalData,
            IUserSettings userSettings)
        {
            if (userSettings.UseLogarithmicGraphScale)
            {
                var minUserData = userData != null ? userData.Values.Min() : 0;
                var minRivalData = rivalData != null ? rivalData.Values.Min() : 0;

                var maxUserData = userData != null ? userData.Values.Max() : 0;
                var maxRivalData = rivalData != null ? rivalData.Values.Max() : 0;

                var hasZeroUser = userData != null ? userData.Values.Any(datum => datum == 0) : false;
                var hasZeroRival = rivalData != null ? rivalData.Values.Any(datum => datum == 0) : false;

                if (Math.Max(maxUserData, maxRivalData) - Math.Min(minUserData, minRivalData) > userSettings.LogarithmicGraphScaleThreshold
                    && !hasZeroUser
                    && !hasZeroRival)
                {
                    return AxisType.Logarithmic;
                }
            }

            return AxisType.Linear;
        }
    }
}