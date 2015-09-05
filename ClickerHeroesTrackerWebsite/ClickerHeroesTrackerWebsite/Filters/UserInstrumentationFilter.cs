namespace ClickerHeroesTrackerWebsite.Filters
{
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNet.Identity;
    using System.Collections.Generic;
    using System.Web;
    using System.Web.Mvc;

    public class UserInstrumentationFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var user = filterContext.HttpContext.User.Identity;

            var telemetry = new TelemetryClient();
            telemetry.TrackEvent(
                "Request",
                new Dictionary<string, string>
                {
                    { "UserId", user.GetUserId() ?? "<anonymous>" },
                    { "UserName", user.GetUserName() ?? "<anonymous>" },
                    { "Referrer", GetReferrer(filterContext.HttpContext.Request) },
                });
        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
        }

        private static string GetReferrer(HttpRequestBase request)
        {
            var referrer = request.UrlReferrer;
            return referrer != null ? referrer.OriginalString : "<none>";
        }
    }
}