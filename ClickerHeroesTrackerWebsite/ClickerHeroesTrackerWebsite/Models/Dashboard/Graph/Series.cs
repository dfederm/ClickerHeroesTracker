namespace ClickerHeroesTrackerWebsite.Models.Dashboard.Graph
{
    using System.Collections.Generic;

    public class Series
    {
        public string Name { get; set; }

        public IList<Point> Data { get; set; }
    }
}