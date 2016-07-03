// <copyright file="MeasureLatencyMiddleware.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Instrumentation
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// A middleware which measures the latency for the request.
    /// </summary>
    public sealed class MeasureLatencyMiddleware
    {
        private readonly RequestDelegate next;

        /// <summary>
        /// Initializes a new instance of the <see cref="MeasureLatencyMiddleware"/> class.
        /// </summary>
        public MeasureLatencyMiddleware(RequestDelegate next)
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
            var counterProvider = ((ICounterProvider)context.RequestServices.GetService(typeof(ICounterProvider)));

            counterProvider.Start(Counter.Total);
            counterProvider.Start(Counter.Internal);
            try
            {
                await this.next.Invoke(context);
            }
            finally
            {
                counterProvider.Stop(Counter.Internal);
                counterProvider.Stop(Counter.Total);
            }
        }
    }
}