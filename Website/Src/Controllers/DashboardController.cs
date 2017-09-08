// <copyright file="DashboardController.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Authorize]
    public class DashboardController : Controller
    {
        public ActionResult Index()
        {
            return this.View();
        }

        public ActionResult Uploads()
        {
            return this.View();
        }

        [AllowAnonymous]
        public ActionResult Progress()
        {
            return this.View();
        }

        [AllowAnonymous]
        public ActionResult Compare()
        {
            return this.View();
        }
    }
}