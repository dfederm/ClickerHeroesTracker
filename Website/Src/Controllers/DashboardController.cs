// <copyright file="DashboardController.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Controllers
{
    using ClickerHeroesTrackerWebsite.Models;
    using ClickerHeroesTrackerWebsite.Models.Dashboard;
    using ClickerHeroesTrackerWebsite.Services.Database;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;

    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IDatabaseCommandFactory databaseCommandFactory;

        private readonly UserManager<ApplicationUser> userManager;

        public DashboardController(
            IDatabaseCommandFactory databaseCommandFactory,
            UserManager<ApplicationUser> userManager)
        {
            this.databaseCommandFactory = databaseCommandFactory;
            this.userManager = userManager;
        }

        public ActionResult Index()
        {
            var model = new DashboardViewModel(
                this.databaseCommandFactory,
                this.User,
                this.userManager);

            return this.View(model);
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