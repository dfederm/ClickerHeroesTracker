// <copyright file="FilterConfig.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite
{
    using System.Web.Mvc;
    using ClickerHeroesTrackerWebsite.Filters;

    /// <summary>
    /// Configures global MVC filters
    /// </summary>
    public static class FilterConfig
    {
        /// <summary>
        /// Registers the global MVC filters
        /// </summary>
        /// <param name="filters">The filter collection</param>
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new MeasureLatencyFilter());
            filters.Add(new HandleAndInstrumentErrorFilter());
        }
    }
}
