namespace ClickerHeroesTrackerWebsite.Models.Dashboard.Graph
{
    public class Axis
    {
        public int? TickInterval { get; set; }

        public AxisType? Type { get; set; }

        public int? TickWidth { get; set; }

        public int? GridLineWidth { get; set; }

        public bool? ShowFirstLabel { get; set; }

        public Labels Labels { get; set; }
    }
}