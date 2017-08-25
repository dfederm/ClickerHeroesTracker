// <copyright file="HomeController.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    // Authorize + AllowAnonymous to basically force Mock authentication to work without requiring it. There's probably a better way for this to work...
    [Authorize]
    [AllowAnonymous]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return this.View();
        }

        public ActionResult New()
        {
            return this.View();
        }

        public ActionResult Beta()
        {
            return this.File("/index.html", "text/html");
        }
    }
}