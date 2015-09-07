namespace ClickerHeroesTrackerWebsite.Filters
{
    using Models;
    using System.Web.Mvc;

    public class HandleAndInstrumentErrorFilter : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            if (filterContext != null && filterContext.HttpContext != null && filterContext.Exception != null)
            {
                if (filterContext.HttpContext.IsCustomErrorEnabled)
                {
                    Telemetry.Client.TrackException(filterContext.Exception);
                }
            }

            base.OnException(filterContext);
        }
    }
}