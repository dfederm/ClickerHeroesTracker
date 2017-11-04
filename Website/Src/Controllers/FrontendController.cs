// <copyright file="FrontendController.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    public class FrontendController : Controller
    {
        public ActionResult Index()
        {
            return this.File("/index.html", "text/html");
        }
    }
}