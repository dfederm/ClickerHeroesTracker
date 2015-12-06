// <copyright file="UserInstrumentationMiddleware.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Instrumentation
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNet.Identity;
    using Microsoft.Owin;

    public class UserInstrumentationMiddleware : OwinMiddleware
    {
        private readonly TelemetryClient telemetryClient;

        public UserInstrumentationMiddleware(OwinMiddleware next, TelemetryClient telemetryClient)
            : base(next)
        {
            this.telemetryClient = telemetryClient;
        }

        /// <inheritdoc/>
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