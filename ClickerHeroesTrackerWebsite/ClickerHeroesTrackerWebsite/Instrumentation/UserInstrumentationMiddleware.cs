namespace ClickerHeroesTrackerWebsite.Instrumentation
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Owin;
    using Microsoft.ApplicationInsights;
    using System.Collections.Generic;
    using Microsoft.AspNet.Identity;

    public class UserInstrumentationMiddleware : OwinMiddleware
    {
        private readonly TelemetryClient telemetryClient;

        public UserInstrumentationMiddleware(OwinMiddleware next, TelemetryClient telemetryClient)
            : base(next)
        {
            this.telemetryClient = telemetryClient;
        }

        public async override Task Invoke(IOwinContext context)
        {
            var user = context.Request.User.Identity;

            this.telemetryClient.TrackEvent(
                "Request",
                new Dictionary<string, string>
                {
                    { "UserId", user.GetUserId() ?? "<anonymous>" },
                    { "UserName", user.GetUserName() ?? "<anonymous>" },
                    { "Referrer", context.Request.Headers["Referer"] ?? "<none>" },
                });

            await this.Next.Invoke(context);
        }
    }
}