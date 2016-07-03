// <copyright file="UserInstrumentationMiddleware.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Instrumentation
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using ClickerHeroesTrackerWebsite.Models;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;

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
            var userManager = ((UserManager<ApplicationUser>)context.RequestServices.GetService(typeof(UserManager<ApplicationUser>)));

            var user = await userManager.GetUserAsync(context.User);
            if (user != null)
            {
                telemetryClient.Context.User.AuthenticatedUserId = user.Id;
            }

            var properties = new Dictionary<string, string>
            {
                { "UserName", user?.UserName ?? "<anonymous>" },
                { "Referrer", context.Request.Headers["Referer"] },
            };
            telemetryClient.TrackEvent("Request", properties);

            await this.next.Invoke(context);
        }
    }
}