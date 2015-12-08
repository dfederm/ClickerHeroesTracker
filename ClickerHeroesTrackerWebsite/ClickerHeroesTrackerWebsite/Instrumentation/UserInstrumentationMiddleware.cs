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

    /// <summary>
    /// An <see cref="OwinMiddleware"/> which instruments the user that made the request.
    /// </summary>
    public class UserInstrumentationMiddleware : OwinMiddleware
    {
        private readonly TelemetryClient telemetryClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserInstrumentationMiddleware"/> class.
        /// </summary>
        public UserInstrumentationMiddleware(OwinMiddleware next, TelemetryClient telemetryClient)
            : base(next)
        {
            this.telemetryClient = telemetryClient;
        }

        /// <inheritdoc/>
        public override Task Invoke(IOwinContext context)
        {
            var user = context.Request.User.Identity;
            var properties = new Dictionary<string, string>
            {
                { "UserId", user.GetUserId() ?? "<anonymous>" },
                { "UserName", user.GetUserName() ?? "<anonymous>" },
                { "Referrer", context.Request.Headers["Referer"] ?? "<none>" },
            };
            this.telemetryClient.TrackEvent("Request", properties);

            return this.Next.Invoke(context);
        }
    }
}