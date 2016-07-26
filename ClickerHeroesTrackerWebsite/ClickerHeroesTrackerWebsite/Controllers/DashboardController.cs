// <copyright file="DashboardController.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Controllers
{
    using System;
    using System.Threading.Tasks;
    using ClickerHeroesTrackerWebsite.Models;
    using ClickerHeroesTrackerWebsite.Models.Dashboard;
    using ClickerHeroesTrackerWebsite.Models.Game;
    using ClickerHeroesTrackerWebsite.Models.Settings;
    using ClickerHeroesTrackerWebsite.Services.Database;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// The Dashboard controller is where the user can see a dashboard of their data.
    /// </summary>
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly GameData gameData;

        private readonly TelemetryClient telemetryClient;

        private readonly IDatabaseCommandFactory databaseCommandFactory;

        private readonly IUserSettingsProvider userSettingsProvider;

        private readonly UserManager<ApplicationUser> userManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="DashboardController"/> class.
        /// </summary>
        public DashboardController(
            GameData gameData,
            TelemetryClient telemetryClient,
            IDatabaseCommandFactory databaseCommandFactory,
            IUserSettingsProvider userSettingsProvider,
            UserManager<ApplicationUser> userManager)
        {
            this.gameData = gameData;
            this.telemetryClient = telemetryClient;
            this.databaseCommandFactory = databaseCommandFactory;
            this.userSettingsProvider = userSettingsProvider;
            this.userManager = userManager;
        }

        /// <summary>
        /// The dashboard homepage
        /// </summary>
        /// <returns>The dashboard view</returns>
        public ActionResult Index()
        {
            var model = new DashboardViewModel(
                this.gameData,
                this.telemetryClient,
                this.databaseCommandFactory,
                this.userSettingsProvider,
                this.User,
                this.userManager);
            if (!model.IsValid)
            {
                this.ViewBag.ErrorMessage = "You have no uploaded data!";
                return this.View("Error");
            }

            return this.View(model);
        }

        /// <summary>
        /// View a list of the user's uploads
        /// </summary>
        /// <returns>The uploads view</returns>
        public ActionResult Uploads()
        {
            return this.View();
        }

        /// <summary>
        /// View the user's progress details
        /// </summary>
        /// <returns>The progress view</returns>
        [AllowAnonymous]
        public ActionResult Progress()
        {
            var userName = this.Request.Query["userName"];
            var range = this.Request.Query["range"];
            var model = new ProgressViewModel(
                this.gameData,
                this.telemetryClient,
                this.databaseCommandFactory,
                this.userSettingsProvider,
                this.User,
                this.userManager,
                userName,
                range);
            if (!model.IsValid)
            {
                this.ViewBag.ErrorMessage = string.IsNullOrEmpty(userName)
                    ? "You have no uploaded data!"
                    : "That user does not exist or does not have public uploads";
                return this.View("Error");
            }

            return this.View(model);
        }

        /// <summary>
        /// Compares two users
        /// </summary>
        /// <returns>The compare view</returns>
        [AllowAnonymous]
        public async Task<IActionResult> Compare(string userName1, string userName2)
        {
            if (string.IsNullOrEmpty(userName1)
                || string.IsNullOrEmpty(userName2))
            {
                this.ViewBag.ErrorMessage = "Two users are required to compare.";
                return this.View("Error");
            }

            var user1 = await this.userManager.FindByNameAsync(userName1);
            if (user1 == null)
            {
                this.ViewBag.ErrorMessage = $"User does not exist: {userName1}.";
                return this.View("Error");
            }

            var user2 = await this.userManager.FindByNameAsync(userName2);
            if (user2 == null)
            {
                this.ViewBag.ErrorMessage = $"User does not exist: {userName2}.";
                return this.View("Error");
            }

            var userId = this.userManager.GetUserId(this.User);
            var userIsAdmin = this.User.IsInRole("Admin");

            var userId1 = await this.userManager.GetUserIdAsync(user1);
            var userId2 = await this.userManager.GetUserIdAsync(user2);

            // Normalize the user names
            userName1 = await this.userManager.GetUserNameAsync(user1);
            userName2 = await this.userManager.GetUserNameAsync(user2);

            if (!userIsAdmin)
            {
                var userSettings1 = userSettingsProvider.Get(userId1);
                if (!userId1.Equals(userId, StringComparison.OrdinalIgnoreCase) && !userSettings1.AreUploadsPublic)
                {
                    this.ViewBag.ErrorMessage = $"{userName1}'s data is private and may not be viewed";
                    return this.View("Error");
                }

                var userSettings2 = userSettingsProvider.Get(userId2);
                if (!userId2.Equals(userId, StringComparison.OrdinalIgnoreCase) && !userSettings2.AreUploadsPublic)
                {
                    this.ViewBag.ErrorMessage = $"{userName2}'s data is private and may not be viewed";
                    return this.View("Error");
                }
            }

            var range = this.Request.Query["range"];
            var model = new CompareViewModel(
                this.gameData,
                this.telemetryClient,
                this.databaseCommandFactory,
                this.userSettingsProvider.Get(userId),
                userId1,
                userName1,
                userId2,
                userName2,
                range);
            if (!model.IsValid)
            {
                this.ViewBag.ErrorMessage = "There was a problem comparing your data to that user. Make sure both you and them have upload data to compare.";
                return this.View("Error");
            }

            return this.View(model);
        }
    }
}