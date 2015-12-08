// <copyright file="MeasureLatencyFilter.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Filters
{
    using System.Web.Mvc;
    using Models;

    /// <summary>
    /// Measures the latency of actions and result executions.
    /// </summary>
    public class MeasureLatencyFilter : IActionFilter, IResultFilter
    {
        /// <inheritdoc/>
        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            LatencyCounter.ActionLatency.Start();
        }

        /// <inheritdoc/>
        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            LatencyCounter.ActionLatency.Record();
        }

        /// <inheritdoc/>
        public void OnResultExecuting(ResultExecutingContext filterContext)
        {
            LatencyCounter.ResultLatency.Start();
        }

        /// <inheritdoc/>
        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
            LatencyCounter.ResultLatency.Record();
        }
    }
}