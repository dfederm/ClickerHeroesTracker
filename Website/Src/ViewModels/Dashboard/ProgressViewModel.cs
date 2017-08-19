// <copyright file="ProgressViewModel.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Dashboard
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
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

            var prominentGraphs = new List<GraphViewModel>();
            this.TryAddGraph(prominentGraphs, "Souls Spent", data.SoulsSpentData, userSettings);
            this.TryAddGraph(prominentGraphs, "Titan Damage", data.TitanDamageData, userSettings);
            this.TryAddGraph(prominentGraphs, "Hero Souls Sacrificed", data.HeroSoulsSacrificedData, userSettings);
            this.TryAddGraph(prominentGraphs, "Total Ancient Souls", data.TotalAncientSoulsData, userSettings);
            this.TryAddGraph(prominentGraphs, "Transcendent Power", data.TranscendentPowerData, userSettings, 2);
            this.TryAddGraph(prominentGraphs, "Rubies", data.RubiesData, userSettings);
            this.TryAddGraph(prominentGraphs, "Highest Zone This Transcension", data.HighestZoneThisTranscensionData, userSettings);
            this.TryAddGraph(prominentGraphs, "Highest Zone Lifetime", data.HighestZoneLifetimeData, userSettings);
            this.TryAddGraph(prominentGraphs, "Ascensions This Transcension", data.AscensionsThisTranscensionData, userSettings);
            this.TryAddGraph(prominentGraphs, "Ascensions Lifetime", data.AscensionsLifetimeData, userSettings);

            var secondaryGraphs = new List<GraphViewModel>();
            foreach (var pair in data.OutsiderLevelData)
            {
                this.TryAddGraph(secondaryGraphs, pair.Key, pair.Value, userSettings);
            }

            foreach (var pair in data.AncientLevelData)
            {
                this.TryAddGraph(secondaryGraphs, pair.Key, pair.Value, userSettings);
            }

            this.ProminentGraphs = prominentGraphs;
            this.SecondaryGraphs = secondaryGraphs;
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

        private static AxisType GetYAxisType(
            IDictionary<DateTime, double> data,
            IUserSettings userSettings)
        {
            return userSettings.UseLogarithmicGraphScale
                && data.Values.Max() - data.Values.Min() > userSettings.LogarithmicGraphScaleThreshold
                ? AxisType.Logarithmic
                : AxisType.Linear;
        }

        private static AxisType GetYAxisType(
            IDictionary<DateTime, BigInteger> data,
            IUserSettings userSettings)
        {
            return userSettings.UseLogarithmicGraphScale
                && data.Values.Max() - data.Values.Min() > userSettings.LogarithmicGraphScaleThreshold
                ? AxisType.Logarithmic
                : AxisType.Linear;
        }

        private void TryAddGraph(
            List<GraphViewModel> graphs,
            string title,
            IDictionary<DateTime, double> data,
            IUserSettings userSettings,
            int numDecimals = 0)
        {
            if (data == null || data.Count == 0)
            {
                return;
            }

            var yAxisType = GetYAxisType(data, userSettings);

            var id = title.Replace(" ", string.Empty, StringComparison.Ordinal).Replace("'", string.Empty, StringComparison.Ordinal) + "Graph";
            graphs.Add(new GraphViewModel
            {
                Id = id,
                Data = new GraphData
                {
                    Chart = new Chart
                    {
                        Type = ChartType.Line,
                    },
                    Title = new Title
                    {
                        Text = title,
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
                            Format = "{value:%m/%d}",
                        },
                    },
                    YAxis = new Axis
                    {
                        Labels = new Labels
                        {
                            Align = Align.Left,
                            X = 3,
                            Y = 16,
                            Format = "{value:,." + numDecimals + "f}",
                        },
                        ShowFirstLabel = false,
                        Type = yAxisType,
                    },
                    Legend = new Legend
                    {
                        Enabled = false,
                    },
                    Series = new Series[]
                    {
                        new Series
                        {
                            Color = Colors.PrimarySeriesColor,
                            Data = data
                                .Select(datum => new Point
                                {
                                    X = datum.Key.ToJavascriptTime(),
                                    Y = datum.Value.ToString("F" + numDecimals),
                                })
                                .Concat(new[]
                                {
                                    new Point
                                    {
                                        X = DateTime.UtcNow.ToJavascriptTime(),
                                        Y = data.Last().Value.ToString("F" + numDecimals),
                                    },
                                })
                                .ToList(),
                        },
                    },
                },
            });
        }

        private void TryAddGraph(
            List<GraphViewModel> graphs,
            string title,
            IDictionary<DateTime, BigInteger> data,
            IUserSettings userSettings,
            int numDecimals = 0)
        {
            if (data == null || data.Count == 0)
            {
                return;
            }

            var yAxisType = GetYAxisType(data, userSettings);

            var id = title.Replace(" ", string.Empty, StringComparison.Ordinal).Replace("'", string.Empty, StringComparison.Ordinal) + "Graph";
            graphs.Add(new GraphViewModel
            {
                Id = id,
                Data = new GraphData
                {
                    Chart = new Chart
                    {
                        Type = ChartType.Line,
                    },
                    Title = new Title
                    {
                        Text = title,
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
                            Format = "{value:%m/%d}",
                        },
                    },
                    YAxis = new Axis
                    {
                        Labels = new Labels
                        {
                            Align = Align.Left,
                            X = 3,
                            Y = 16,
                            Format = "{value:,." + numDecimals + "f}",
                        },
                        ShowFirstLabel = false,
                        Type = yAxisType,
                    },
                    Legend = new Legend
                    {
                        Enabled = false,
                    },
                    Series = new Series[]
                    {
                        new Series
                        {
                            Color = Colors.PrimarySeriesColor,
                            Data = data
                                .Select(datum => new Point
                                {
                                    X = datum.Key.ToJavascriptTime(),
                                    Y = datum.Value.ToString("F" + numDecimals),
                                })
                                .Concat(new[]
                                {
                                    new Point
                                    {
                                        X = DateTime.UtcNow.ToJavascriptTime(),
                                        Y = data.Last().Value.ToString("F" + numDecimals),
                                    },
                                })
                                .ToList(),
                        },
                    },
                },
            });
        }
    }
}