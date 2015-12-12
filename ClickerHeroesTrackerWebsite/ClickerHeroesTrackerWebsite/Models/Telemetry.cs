// <copyright file="Telemetry.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models
{
    using System.Web;
    using Microsoft.ApplicationInsights;

    internal static class Telemetry
    {
        // BUGBUG 56 - Retrieve via DI
        public static TelemetryClient Client
        {
            get
            {
                return HttpContext.Current.Items["TelemetryClient"] as TelemetryClient
                    ?? (TelemetryClient)(HttpContext.Current.Items["TelemetryClient"] = new TelemetryClient());
            }
        }
    }
}