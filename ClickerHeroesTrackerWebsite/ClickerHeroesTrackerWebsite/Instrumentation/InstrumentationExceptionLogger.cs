// <copyright file="InstrumentationExceptionLogger.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Instrumentation
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http.ExceptionHandling;
    using Microsoft.ApplicationInsights;

    /// <summary>
    /// An exception logger which logs exceptions.
    /// </summary>
    public class InstrumentationExceptionLogger : ExceptionLogger
    {
        // Must be a Func since this is a singleton and TelemetryClient has PerRequest lifetime.
        private readonly Func<TelemetryClient> telemetryClientResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="InstrumentationExceptionLogger"/> class.
        /// </summary>
        public InstrumentationExceptionLogger(Func<TelemetryClient> telemetryClientResolver)
        {
            this.telemetryClientResolver = telemetryClientResolver;
        }

        /// <inheritdoc />
        public override void Log(ExceptionLoggerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // Capture additional properties
            var properties = new Dictionary<string, string>();
            var request = context.Request;
            var headersString = new StringBuilder();
            foreach (var pair in request.Headers)
            {
                headersString.AppendFormat($"{pair.Key}: {string.Join(";", pair.Value)}{Environment.NewLine}");
            }

            properties.Add("Headers", headersString.ToString());

            properties.Add("Logger", "InstrumentationExceptionLogger");

            // Instrument
            this.telemetryClientResolver().TrackException(context.Exception, properties);
        }
    }
}