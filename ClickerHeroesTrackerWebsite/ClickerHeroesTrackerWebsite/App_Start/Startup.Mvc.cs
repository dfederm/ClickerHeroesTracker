// <copyright file="Startup.Mvc.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite
{
    using System.Web.Http;
    using System.Web.Mvc;
    using System.Web.Optimization;
    using System.Web.Routing;
    using Instrumentation;
    using Microsoft.Practices.Unity;

    /// <summary>
    /// Configure Mvc
    /// </summary>
    public partial class Startup
    {
        /// <summary>
        /// Configure Mvc
        /// </summary>
        /// <param name="container">The unity container</param>
        private static void ConfigureMvc(IUnityContainer container)
        {
            RegisterGlobalFilters(GlobalFilters.Filters, container);
            RegisterRoutes(RouteTable.Routes);
            RegisterBundles(BundleTable.Bundles);
        }

        /// <summary>
        /// Registers the global MVC filters
        /// </summary>
        /// <param name="filters">The filter collection</param>
        /// <param name="container">The unity container</param>
        private static void RegisterGlobalFilters(GlobalFilterCollection filters, IUnityContainer container)
        {
            filters.Add(container.Resolve<MeasureLatencyFilter>());
            filters.Add(container.Resolve<HandleAndInstrumentErrorFilter>());
        }

        /// <summary>
        /// Registers the MVC routes
        /// </summary>
        /// <param name="routes">The route collection</param>
        private static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional });
        }

        /// <summary>
        /// Registers all bundles
        /// </summary>
        /// <remarks>For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862</remarks>
        /// <param name="bundles">The bundle collection</param>
        private static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery")
                .Include("~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval")
                .Include("~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr")
                .Include("~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap")
                .Include("~/Scripts/bootstrap.js", "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css")
                .Include("~/Content/bootstrap.css", "~/Content/site.css"));
        }
    }
}