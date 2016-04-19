// <copyright file="MeasureLatencyFilter.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Instrumentation
{
    using Microsoft.AspNet.Mvc.Filters;

    /// <summary>
    /// Measures the latency of actions and result executions.
    /// </summary>
    public class MeasureLatencyFilter : IActionFilter, IResultFilter
    {
        private readonly ICounterProvider counterProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="MeasureLatencyFilter"/> class.
        /// </summary>
        public MeasureLatencyFilter(ICounterProvider counterProvider)
        {
            this.counterProvider = counterProvider;
        }

        /// <inheritdoc/>
        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            this.counterProvider.Start(Counter.Action);
        }

        /// <inheritdoc/>
        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            this.counterProvider.Stop(Counter.Action);
        }

        /// <inheritdoc/>
        public void OnResultExecuting(ResultExecutingContext filterContext)
        {
            this.counterProvider.Start(Counter.Result);
        }

        /// <inheritdoc/>
        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
            this.counterProvider.Stop(Counter.Result);
        }
    }
}