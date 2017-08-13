// <copyright file="ErrorController.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Handles the error page.
    /// </summary>
    public class ErrorController : Controller
    {
        /// <summary>
        /// The error page for the site.
        /// </summary>
        /// <returns>The error view</returns>
        public ActionResult Index()
        {
            return this.View("Error");
        }
    }
}