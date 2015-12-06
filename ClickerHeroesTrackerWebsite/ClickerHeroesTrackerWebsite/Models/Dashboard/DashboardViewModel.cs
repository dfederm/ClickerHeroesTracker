namespace ClickerHeroesTrackerWebsite.Models.Dashboard
{
    using Microsoft.AspNet.Identity;
    using System;
    using System.Security.Principal;
    using Graph;
    using System.Linq;
    using Database;
    using System.Data;
    using System.Collections.Generic;
    using Settings;

    public class DashboardViewModel
    {
        public DashboardViewModel(
            IDatabaseCommandFactory databaseCommandFactory,
            IUserSettingsProvider userSettingsProvider,
            IPrincipal user)
        {
            var userId = user.Identity.GetUserId();

            var userSettings = userSettingsProvider.Get(userId);

            var startTime = DateTime.UtcNow.AddDays(-7);

            ProgressData data;
            using (var command = databaseCommandFactory.Create(
                "GetProgressData",
                CommandType.StoredProcedure,
                new Dictionary<string, object>
                {
                    { "@UserId", userId },
                    { "@StartTime", startTime },
                }))
            using (var reader = command.ExecuteReader())
            {
                data = new ProgressData(reader, userSettings);
            }

            if (data.SoulsPerHourData.Count > 0)
            {
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
            }

            this.RivalDataList = new RivalDataList(databaseCommandFactory, userId);

            this.IsValid = data.IsValid;
        }

        public bool IsValid { get; private set; }

        public GraphData ProgressSummaryGraph { get; private set; }

        public RivalDataList RivalDataList { get; private set; }
    }
}