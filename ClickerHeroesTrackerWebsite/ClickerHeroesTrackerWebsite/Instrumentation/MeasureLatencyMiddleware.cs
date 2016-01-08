// <copyright file="MeasureLatencyMiddleware.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Instrumentation
{
    using System.Threading.Tasks;
    using Microsoft.Owin;

    /// <summary>
    /// An <see cref="OwinMiddleware"/> which measures the latency for the request.
    /// </summary>
    public sealed class MeasureLatencyMiddleware : OwinMiddleware
    {
        private readonly ICounterProvider counterProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="MeasureLatencyMiddleware"/> class.
        /// </summary>
        public MeasureLatencyMiddleware(OwinMiddleware next, ICounterProvider counterProvider)
            : base(next)
        {
            this.counterProvider = counterProvider;
        }

        /// <inheritdoc/>
        public override Task Invoke(IOwinContext context)
        {
            using (this.counterProvider.Measure(Counter.Total))
            using (this.counterProvider.Measure(Counter.Internal))
            {
                // To ensure we measure the entire operation, we need to wait for it to complete.
                this.Next.Invoke(context).Wait();
            }

            return Task.CompletedTask;
        }
    }
}