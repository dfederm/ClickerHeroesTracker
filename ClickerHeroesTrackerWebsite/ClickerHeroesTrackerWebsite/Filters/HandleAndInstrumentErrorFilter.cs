namespace ClickerHeroesTrackerWebsite.Filters
{
    using Microsoft.ApplicationInsights;
    using System.Web.Mvc;

    public class HandleAndInstrumentErrorFilter : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            if (filterContext != null && filterContext.HttpContext != null && filterContext.Exception != null)
            {
                if (filterContext.HttpContext.IsCustomErrorEnabled)
                {
                    var telemetry = new TelemetryClient();
                    telemetry.TrackException(filterContext.Exception);
                }
            }

            base.OnException(filterContext);
        }
    }
}