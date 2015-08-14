namespace ClickerHeroesTrackerWebsite.Models.Dashboard
{
    using System;
    using System.Collections.Generic;

    public class GraphData
    {
        public string Id { get; set; }

        public TimeZoneInfo TimeZone { get; set; }

        public string Title { get; set; }

        public string Series1Name { get; set; }

        public IDictionary<DateTime, int> Series1 { get; set; }

        public string Series2Name { get; set; }

        public IDictionary<DateTime, int> Series2 { get; set; }
    }
}