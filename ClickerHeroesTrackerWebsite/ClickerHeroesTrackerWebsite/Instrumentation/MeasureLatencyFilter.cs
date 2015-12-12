// <copyright file="MeasureLatencyFilter.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Instrumentation
{
    using System;
    using System.Web.Mvc;

    /// <summary>
    /// Measures the latency of actions and result executions.
    /// </summary>
    public class MeasureLatencyFilter : IActionFilter, IResultFilter
    {
        // Must be a Func since the filter is a singleton and ICounterProvider has PerRequest lifetime.
        private readonly Func<ICounterProvider> counterProviderResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="MeasureLatencyFilter"/> class.
        /// </summary>
        public MeasureLatencyFilter(Func<ICounterProvider> counterProviderResolver)
        {
            this.counterProviderResolver = counterProviderResolver;
        }

        /// <inheritdoc/>
        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            this.counterProviderResolver().Start(Counter.Action);
        }

        /// <inheritdoc/>
        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            this.counterProviderResolver().Stop(Counter.Action);
        }

        /// <inheritdoc/>
        public void OnResultExecuting(ResultExecutingContext filterContext)
        {
            this.counterProviderResolver().Start(Counter.Result);
        }

        /// <inheritdoc/>
        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
            this.counterProviderResolver().Stop(Counter.Result);
        }
    }
}