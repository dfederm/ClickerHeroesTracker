﻿// <copyright file="ProgressViewModel.cs" company="Clicker Heroes Tracker">
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
    using ClickerHeroesTrackerWebsite.ViewModels.Dashboard.Graph;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Identity;

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
            UserManager<ApplicationUser> userManager,
            string progressUserName,
            string range)
        {
            this.ProgressUserName = progressUserName;

            var userId = userManager.GetUserId(user);
            string progressUserId = null;
            if (string.IsNullOrEmpty(progressUserName))
            {
                progressUserId = userId;
            }
            else
            {
                var progressUser = AsyncHelper.RunSynchronously(async () => await userManager.FindByNameAsync(progressUserName));
                if (progressUser != null)
                {
                    progressUserId = AsyncHelper.RunSynchronously(async () => await userManager.GetUserIdAsync(progressUser));
                }
            }

            if (string.IsNullOrEmpty(progressUserId))
            {
                // If we didn't get data, it's a user that doesn't exist
                return;
            }

            var progressUserSettings = userSettingsProvider.Get(progressUserId);

            if (!progressUserId.Equals(userId, StringComparison.OrdinalIgnoreCase)
                && !progressUserSettings.AreUploadsPublic
                && !user.IsInRole("Admin"))
            {
                // Not permitted
                return;
            }

            var userSettings = userSettingsProvider.Get(userId);

            this.RangeSelector = new GraphRangeSelectorViewModel(range);

            var data = new ProgressData(
                gameData,
                telemetryClient,
                databaseCommandFactory,
                progressUserId,
                this.RangeSelector.Start,
                this.RangeSelector.End);

            if (!data.IsValid)
            {
                return;
            }

            this.ProminentGraphs = new List<GraphViewModel>
            {
                this.CreateGraph(
                    "soulsSpentGraph",
                    "Souls Spent",
                    data.SoulsSpentData,
                    userSettings),
                this.CreateGraph(
                    "titanDamageGraph",
                    "Titan Damage",
                    data.TitanDamageData,
                    userSettings),
            };

            this.SecondaryGraphs = data
                .AncientLevelData
                .Select(x => this.CreateGraph(
                    x.Key.Name + "Graph",
                    x.Key.Name,
                    x.Value,
                    userSettings))
                .ToList();

            this.IsValid = true;
        }

        /// <summary>
        /// Gets the user name for the user whose progress this is.
        /// </summary>
        public string ProgressUserName { get; }

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
            IDictionary<DateTime, double> data,
            IUserSettings userSettings)
        {
            var timeZone = userSettings.TimeZone;

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
                        Type = GetYAxisType(data, userSettings),
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