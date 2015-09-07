namespace ClickerHeroesTrackerWebsite.Filters
{
    using Models;
    using System.Data.SqlClient;
    using System.Web.Mvc;

    public class DatabaseConnectionClosingFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var connection = filterContext.HttpContext.Items["SqlConnection"] as SqlConnection;
            if (connection != null)
            {
                Telemetry.Client.TrackEvent("SqlConnectionClose");
                connection.Dispose();
                filterContext.HttpContext.Items.Remove("SqlConnection");
            }
        }
    }
}