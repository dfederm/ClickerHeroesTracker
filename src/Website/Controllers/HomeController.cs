// <copyright file="HomeController.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Handles the homepage and other related pages.
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// The homepage for the site.
        /// </summary>
        /// <returns>The homepage view</returns>
        public ActionResult Index()
        {
            return this.View();
        }

        /// <summary>
        /// The "what's new" page
        /// </summary>
        /// <returns>The "what's new" view</returns>
        public ActionResult New()
        {
            return this.View();
        }
    }
}