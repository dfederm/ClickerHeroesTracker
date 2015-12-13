// <copyright file="DashboardViewModel.cs" company="Clicker Heroes Tracker">
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
    using Graph;
    using Microsoft.AspNet.Identity;
    using Settings;

    /// <summary>
    /// The model for the dashboard view.
    /// </summary>
    public class DashboardViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DashboardViewModel"/> class.
        /// </summary>
        public DashboardViewModel(
            IDatabaseCommandFactory databaseCommandFactory,
            IUserSettingsProvider userSettingsProvider,
            IPrincipal user)
        {
            var userId = user.Identity.GetUserId();

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
                data = new ProgressData(reader, userSettings);
            }

            if (!data.IsValid || data.SoulsPerHourData.Count == 0)
            {
                return;
            }

            this.ProgressSummaryGraph = new GraphData
            {
                Chart = new Chart
                {
                    Type = ChartType.Line
                },
                Title = new Title
                {
                    Text = "Souls/hr"
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
                            .SoulsPerHourData
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
                                    Y = data.SoulsPerHourData.Last().Value
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
    }
}