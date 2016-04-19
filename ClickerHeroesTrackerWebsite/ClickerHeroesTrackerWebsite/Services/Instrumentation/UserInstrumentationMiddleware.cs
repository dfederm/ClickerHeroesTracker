// <copyright file="UserInstrumentationMiddleware.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Instrumentation
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNet.Builder;
    using Microsoft.AspNet.Http;

    /// <summary>
    /// A middleware which instruments the user that made the request.
    /// </summary>
    public sealed class UserInstrumentationMiddleware
    {
        private readonly RequestDelegate next;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserInstrumentationMiddleware"/> class.
        /// </summary>
        public UserInstrumentationMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        /// <summary>
        /// Executes this middleware
        /// </summary>
        /// <param name="context">The current http context</param>
        /// <returns>The async task</returns>
        public async Task Invoke(HttpContext context)
        {
            var telemetryClient = ((TelemetryClient)context.RequestServices.GetService(typeof(TelemetryClient)));

            var user = context.User;
            var userId = user.GetUserId();
            if (userId != null)
            {
                telemetryClient.Context.User.AuthenticatedUserId = userId;
            }

            var properties = new Dictionary<string, string>
            {
                { "UserName", user.GetUserName() ?? "<anonymous>" },
                { "Referrer", context.Request.Headers["Referer"] },
            };
            telemetryClient.TrackEvent("Request", properties);

            await this.next.Invoke(context);
        }
    }
}