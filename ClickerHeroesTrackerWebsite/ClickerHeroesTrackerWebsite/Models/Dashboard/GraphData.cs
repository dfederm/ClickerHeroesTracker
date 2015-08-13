namespace ClickerHeroesTrackerWebsite.Models.Dashboard
{
    using System;
    using System.Collections.Generic;

    public class GraphData
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public IDictionary<DateTime, int> Data { get; set; }
    }
}