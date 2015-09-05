namespace ClickerHeroesTrackerWebsite
{
    using ClickerHeroesTrackerWebsite.Filters;
    using System.Web.Mvc;

    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleAndInstrumentErrorFilter());
            filters.Add(new UserInstrumentationFilter());
        }
    }
}
