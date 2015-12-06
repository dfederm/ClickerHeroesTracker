namespace ClickerHeroesTrackerWebsite.Models.Dashboard
{
    using Settings;
    using Database;
    using Graph;
    using Microsoft.AspNet.Identity;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Security.Principal;

    public class RivalViewModel
    {
        public RivalViewModel(
            IDatabaseCommandFactory databaseCommandFactory,
            IUserSettingsProvider userSettingsProvider,
            IPrincipal user,
            int rivalId)
        {
            var userId = user.Identity.GetUserId();
            var userName = user.Identity.GetUserName();

            var userSettings = userSettingsProvider.Get(userId);

            var startTime = DateTime.UtcNow.AddDays(-7);

            ProgressData userData;
            ProgressData rivalData;

            using (var command = databaseCommandFactory.Create(
                "GetRivalData",
                CommandType.StoredProcedure,
                new Dictionary<string, object>
                {
                    { "@UserId", userId },
                    { "@RivalId", rivalId },
                    { "@StartTime", startTime },
                }))
            using (var reader = command.ExecuteReader())
            {
                userData = new ProgressData(reader, userSettings);
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

                rivalData = new ProgressData(reader, userSettings);
            }

            this.ProminentGraphs = new List<GraphViewModel>
            {
                CreateGraph(
                    "soulsPerHourGraph",
                    "Souls/hr",
                    userName,
                    userData.SoulsPerHourData,
                    this.RivalUserName,
                    rivalData.SoulsPerHourData,
                    userSettings.TimeZone),
                CreateGraph(
                    "optimalLevelGraph",
                    "Optimal Level",
                    userName,
                    userData.OptimalLevelData,
                    this.RivalUserName,
                    rivalData.OptimalLevelData,
                    userSettings.TimeZone),
                CreateGraph(
                    "soulsSpentGraph",
                    "Souls Spent",
                    userName,
                    userData.SoulsSpentData,
                    this.RivalUserName,
                    rivalData.SoulsSpentData,
                    userSettings.TimeZone),
                CreateGraph(
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
                .Select(x => CreateGraph(
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

        public bool IsValid { get; private set; }

        public string RivalUserName { get; private set; }

        public IList<GraphViewModel> ProminentGraphs { get; private set; }

        public IList<GraphViewModel> SecondaryGraphs { get; private set; }

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