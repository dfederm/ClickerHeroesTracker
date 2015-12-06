// <copyright file="RouteConfig.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite
{
    using System.Web.Mvc;
    using System.Web.Routing;

    /// <summary>
    /// Configures MVC routes
    /// </summary>
    public static class RouteConfig
    {
        /// <summary>
        /// Registers the MVC routes
        /// </summary>
        /// <param name="routes">The route collection</param>
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional });
        }
    }
}
