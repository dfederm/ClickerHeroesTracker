namespace ClickerHeroesTrackerWebsite.Filters
{
    using Models;
    using System.Web.Mvc;

    public class MeasureLatencyFilter : IActionFilter, IResultFilter
    {
        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            LatencyCounter.ActionLatency.Start();
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            LatencyCounter.ActionLatency.Record();
        }

        public void OnResultExecuting(ResultExecutingContext filterContext)
        {
            LatencyCounter.ResultLatency.Start();
        }

        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
            LatencyCounter.ResultLatency.Record();
        }
    }
}