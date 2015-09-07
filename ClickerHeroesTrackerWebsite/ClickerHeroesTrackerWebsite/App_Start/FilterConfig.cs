namespace ClickerHeroesTrackerWebsite
{
    using ClickerHeroesTrackerWebsite.Filters;
    using System.Web.Mvc;

    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            // Make sure this one is first to best measure all latency
            filters.Add(new MeasureLatencyFilter());

            filters.Add(new HandleAndInstrumentErrorFilter());
            filters.Add(new UserInstrumentationFilter());
            filters.Add(new DatabaseConnectionClosingFilter());
        }
    }
}
