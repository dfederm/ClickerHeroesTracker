namespace ClickerHeroesTrackerWebsite.Models
{
    using Microsoft.ApplicationInsights;
    using System.Web;

    public class Telemetry
    {
        private static TelemetryClient client = new TelemetryClient();

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