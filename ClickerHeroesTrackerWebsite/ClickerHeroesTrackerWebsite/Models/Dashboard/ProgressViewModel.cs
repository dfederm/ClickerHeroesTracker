namespace ClickerHeroesTrackerWebsite.Models.Dashboard
{
    using Graph;
    using Microsoft.AspNet.Identity;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Principal;

    public class ProgressViewModel
    {
        public ProgressViewModel(IPrincipal user)
        {
            var userId = user.Identity.GetUserId();

            var userSettings = new UserSettings(userId);
            userSettings.Fill();

            var startTime = DateTime.UtcNow.AddDays(-7);
            ProgressData data;
            using (var command = new DatabaseCommand("GetProgressData"))
            {
                command.AddParameter("@UserId", userId);
                command.AddParameter("@StartTime", startTime);

                var reader = command.ExecuteReader();

                data = new ProgressData(reader, userSettings);
            }

            this.ProminentGraphs = new List<GraphViewModel>
            {
                CreateGraph(
                    "soulsPerHourGraph",
                    "Souls/hr",
                    data.SoulsPerHourData,
                    userSettings.TimeZone),
                CreateGraph(
                    "optimalLevelGraph",
                    "Optimal Level",
                    data.OptimalLevelData,
                    userSettings.TimeZone),
                CreateGraph(
                    "soulsSpentGraph",
                    "Souls Spent",
                    data.SoulsSpentData,
                    userSettings.TimeZone),
                CreateGraph(
                    "titanDamageGraph",
                    "Titan Damage",
                    data.TitanDamageData,
                    userSettings.TimeZone),
            };
            this.SecondaryGraphs = data
                .AncientLevelData
                .Select(x => CreateGraph(
                    x.Key.Name + "Graph",
                    x.Key.Name,
                    x.Value,
                    userSettings.TimeZone))
                .ToList();

            this.IsValid = data.IsValid;
        }

        public IList<GraphViewModel> ProminentGraphs { get; private set; }

        public IList<GraphViewModel> SecondaryGraphs { get; private set; }

        public bool IsValid { get; private set; }

        private GraphViewModel CreateGraph(
            string id,
            string title,
            IDictionary<DateTime, long> data,
            TimeZoneInfo timeZone)
        {
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
                    Series = new Series[]
                    {
                        new Series
                        {
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
    }
}