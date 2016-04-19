// <copyright file="DashboardController.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Controllers
{
    using Database;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNet.Authorization;
    using Microsoft.AspNet.Mvc;
    using Models.Dashboard;
    using Models.Game;
    using Models.Settings;

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

        /// <summary>
        /// Initializes a new instance of the <see cref="DashboardController"/> class.
        /// </summary>
        public DashboardController(
            GameData gameData,
            TelemetryClient telemetryClient,
            IDatabaseCommandFactory databaseCommandFactory,
            IUserSettingsProvider userSettingsProvider)
        {
            this.gameData = gameData;
            this.telemetryClient = telemetryClient;
            this.databaseCommandFactory = databaseCommandFactory;
            this.userSettingsProvider = userSettingsProvider;
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
                this.User);
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
        public ActionResult Progress()
        {
            var range = this.Request.Query["range"];
            var model = new ProgressViewModel(
                this.gameData,
                this.telemetryClient,
                this.databaseCommandFactory,
                this.userSettingsProvider,
                this.User,
                range);
            if (!model.IsValid)
            {
                this.ViewBag.ErrorMessage = "You have no uploaded data!";
                return this.View("Error");
            }

            return this.View(model);
        }

        /// <summary>
        /// View the user's progress compared with a rival's
        /// </summary>
        /// <returns>The rival view</returns>
        public ActionResult Rival()
        {
            var rivalIdRaw = this.Request.Query["rivalId"];
            int rivalId;
            if (!int.TryParse(rivalIdRaw, out rivalId))
            {
                return this.RedirectToAction("Index");
            }

            var range = this.Request.Query["range"];
            var model = new RivalViewModel(
                this.gameData,
                this.telemetryClient,
                this.databaseCommandFactory,
                this.userSettingsProvider,
                this.User,
                rivalId,
                range);
            if (!model.IsValid)
            {
                this.ViewBag.ErrorMessage = "There was a problem comparing your data to that rival. Man sure they're your rival and have upload data.";
                return this.View("Error");
            }

            return this.View(model);
        }
    }
}