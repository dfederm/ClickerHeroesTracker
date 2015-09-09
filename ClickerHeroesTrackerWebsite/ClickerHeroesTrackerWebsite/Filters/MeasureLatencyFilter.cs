namespace ClickerHeroesTrackerWebsite.Filters
{
    using Models;
    using System.Web.Mvc;

    public class MeasureLatencyFilter : IActionFilter, IResultFilter, IExceptionFilter
    {
        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            LatencyCounter.TotalLatency.Start();
            LatencyCounter.InternalLatency.Start();
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
            LatencyCounter.InternalLatency.Record();
            LatencyCounter.DependencyLatency.Record();
            LatencyCounter.TotalLatency.Record();
        }

        public void OnException(ExceptionContext filterContext)
        {
            // These counters may not be finished yet, so just try recording them.
            LatencyCounter.InternalLatency.Record();
            LatencyCounter.DependencyLatency.Record();
            LatencyCounter.TotalLatency.Record();
        }
    }
}