// <copyright file="RequireHttpsMiddleware.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Services.Security
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Extensions;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;

    public sealed class RequireHttpsMiddleware
    {
        private readonly RequestDelegate next;

        public RequireHttpsMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public Task Invoke(HttpContext context)
        {
            if (context.Request.IsHttps)
            {
                return this.next.Invoke(context);
            }

            var telemetryClient = (TelemetryClient)context.RequestServices.GetService(typeof(TelemetryClient));

            // only redirect for GET and HEAD requests, otherwise the browser might not propagate the verb and request body correctly
            if (!string.Equals(context.Request.Method, "GET", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(context.Request.Method, "HEAD", StringComparison.OrdinalIgnoreCase))
            {
                telemetryClient.TrackEvent("RequireHttps-Forbidden");
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            }

            telemetryClient.TrackEvent("RequireHttps-Redirect");

            var optionsAccessor = (IOptions<MvcOptions>)context.RequestServices.GetService(typeof(IOptions<MvcOptions>));
            var uriBuilder = new UriBuilder(context.Request.GetEncodedUrl())
            {
                Scheme = Uri.UriSchemeHttps,
                Port = optionsAccessor.Value.SslPort.GetValueOrDefault(-1),
            };

            context.Response.Redirect(uriBuilder.ToString(), permanent: true);

            return Task.CompletedTask;
        }
    }
}
