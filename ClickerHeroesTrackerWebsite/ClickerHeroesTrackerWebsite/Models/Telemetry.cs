namespace ClickerHeroesTrackerWebsite.Models
{
    using Microsoft.ApplicationInsights;
    using System.Web;

    public class Telemetry
    {
        // BUGBUG 56 - Retrieve via DI
        public static TelemetryClient Client
        {
            get
            {
                return HttpContext.Current.Items["TelemetryClient"] as TelemetryClient
                    ?? (TelemetryClient)(HttpContext.Current.Items["TelemetryClient"] = new TelemetryClient()); ;
            }
        }
    }
}