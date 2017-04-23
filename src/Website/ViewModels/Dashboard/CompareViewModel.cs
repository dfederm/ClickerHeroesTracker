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
    using ClickerHeroesTrackerWebsite.Utility;
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
            if (!userData1.IsValid || !userData2.IsValid)
            {
                return;
            }

            var prominentGraphs = new List<GraphViewModel>();
            this.TryAddGraph(prominentGraphs, "Souls Spent", this.UserName1, userData1.SoulsSpentData, this.UserName2, userData2.SoulsSpentData, userSettings);
            this.TryAddGraph(prominentGraphs, "Titan Damage", this.UserName1, userData1.TitanDamageData, this.UserName2, userData2.TitanDamageData, userSettings);
            this.TryAddGraph(prominentGraphs, "Hero Souls Sacrificed", this.UserName1, userData1.HeroSoulsSacrificedData, this.UserName2, userData2.HeroSoulsSacrificedData, userSettings);
            this.TryAddGraph(prominentGraphs, "Total Ancient Souls", this.UserName1, userData1.TotalAncientSoulsData, this.UserName2, userData2.TotalAncientSoulsData, userSettings);
            this.TryAddGraph(prominentGraphs, "Transcendent Power", this.UserName1, userData1.TranscendentPowerData, this.UserName2, userData2.TranscendentPowerData, userSettings, 2);
            this.TryAddGraph(prominentGraphs, "Rubies", this.UserName1, userData1.RubiesData, this.UserName2, userData2.RubiesData, userSettings);
            this.TryAddGraph(prominentGraphs, "Highest Zone This Transcension", this.UserName1, userData1.HighestZoneThisTranscensionData, this.UserName2, userData2.HighestZoneThisTranscensionData, userSettings);
            this.TryAddGraph(prominentGraphs, "Highest Zone Lifetime", this.UserName1, userData1.HighestZoneLifetimeData, this.UserName2, userData2.HighestZoneLifetimeData, userSettings);
            this.TryAddGraph(prominentGraphs, "Ascensions This Transcension", this.UserName1, userData1.AscensionsThisTranscensionData, this.UserName2, userData2.AscensionsThisTranscensionData, userSettings);
            this.TryAddGraph(prominentGraphs, "Ascensions Lifetime", this.UserName1, userData1.AscensionsLifetimeData, this.UserName2, userData2.AscensionsLifetimeData, userSettings);

            var secondaryGraphs = new List<GraphViewModel>();
            foreach (var pair in userData1.OutsiderLevelData)
            {
                this.TryAddGraph(secondaryGraphs, pair.Key, this.UserName1, pair.Value, this.UserName2, userData2.OutsiderLevelData.SafeGet(pair.Key), userSettings);
            }

            foreach (var pair in userData1.AncientLevelData)
            {
                this.TryAddGraph(secondaryGraphs, pair.Key, this.UserName1, pair.Value, this.UserName2, userData2.AncientLevelData.SafeGet(pair.Key), userSettings);
            }

            this.ProminentGraphs = prominentGraphs;
            this.SecondaryGraphs = secondaryGraphs;
            this.IsValid = true;
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

        private static bool TryAddSeries(
            List<Series> series,
            string name,
            IDictionary<DateTime, double> data,
            string color,
            int numDecimals,
            AxisType yAxisType)
        {
            if (data == null || data.Count == 0)
            {
                return false;
            }

            // If we're using a log scale, hack around the inability to plot a 0 value
            // by changing it to 0.1 (1e-1) or "one below" 1 (1e0).
            if (yAxisType == AxisType.Logarithmic)
            {
                // Defer the modifications until after we're done iterating to avoid an InvalidOperationException.
                var actions = new List<Action>();
                foreach (var pair in data)
                {
                    if (pair.Value == 0)
                    {
                        actions.Add(() => data[pair.Key] = 0.1);
                    }
                }

                for (var i = 0; i < actions.Count; i++)
                {
                    actions[i]();
                }
            }

            series.Add(new Series
            {
                Name = name,
                Color = color,
                Data = data
                    .Select(datum => new Point
                    {
                        X = datum.Key.ToJavascriptTime(),
                        Y = datum.Value,
                        YFormat = "F" + (datum.Value == 0.1 ? 1 : numDecimals),
                    })
                    .Concat(new[]
                    {
                        new Point
                        {
                            X = DateTime.UtcNow.ToJavascriptTime(),
                            Y = data.Last().Value,
                            YFormat = "F" + (data.Last().Value == 0.1 ? 1 : numDecimals),
                        },
                    })
                    .ToList(),
            });
            return true;
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

                if (Math.Max(maxUserData1, maxUserData2) - Math.Min(minUserData1, minUserData2) > userSettings.LogarithmicGraphScaleThreshold)
                {
                    return AxisType.Logarithmic;
                }
            }

            return AxisType.Linear;
        }

        private void TryAddGraph(
            List<GraphViewModel> graphs,
            string title,
            string userName1,
            IDictionary<DateTime, double> userData1,
            string userName2,
            IDictionary<DateTime, double> userData2,
            IUserSettings userSettings,
            int numDecimals = 0)
        {
            var yAxisType = GetYAxisType(userData1, userData2, userSettings);

            var series = new List<Series>();
            var user1Added = TryAddSeries(series, userName1, userData1, Colors.PrimarySeriesColor, numDecimals, yAxisType);
            var user2Added = TryAddSeries(series, userName2, userData2, Colors.OpposingSeriesColor, numDecimals, yAxisType);
            if (!user1Added && !user2Added)
            {
                return;
            }

            var id = title.Replace(" ", string.Empty).Replace("'", string.Empty) + "Graph";
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
                    Series = series,
                },
            });
        }
    }
}