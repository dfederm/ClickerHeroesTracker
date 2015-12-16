// <copyright file="CalculatorController.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Controllers
{
    using System.Web.Mvc;
    using Database;
    using Microsoft.ApplicationInsights;
    using Models.Calculator;
    using Models.Game;
    using Models.Settings;
    using Models.Upload;

    /// <summary>
    /// The calculator controller shows stats for a given upload.
    /// </summary>
    public class CalculatorController : Controller
    {
        private readonly IDatabaseCommandFactory databaseCommandFactory;

        private readonly IUserSettingsProvider userSettingsProvider;

        private readonly GameData gameData;

        private readonly TelemetryClient telemetryClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="CalculatorController"/> class.
        /// </summary>
        public CalculatorController(
            IDatabaseCommandFactory databaseCommandFactory,
            IUserSettingsProvider userSettingsProvider,
            GameData gameData,
            TelemetryClient telemetryClient)
        {
            this.databaseCommandFactory = databaseCommandFactory;
            this.userSettingsProvider = userSettingsProvider;
            this.gameData = gameData;
            this.telemetryClient = telemetryClient;
        }

        /// <summary>
        /// Commit an upload to the database and show the calculator for that upload.
        /// </summary>
        /// <param name="uploadViewModel">The user-submitted upload data</param>
        /// <returns>The calculator view</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Upload(UploadViewModel uploadViewModel)
        {
            if (!this.ModelState.IsValid)
            {
                return this.RedirectToAction("Index", "Upload");
            }

            var model = new CalculatorViewModel(
                this.databaseCommandFactory,
                this.userSettingsProvider,
                this.gameData,
                this.telemetryClient,
                uploadViewModel.EncodedSaveData,
                this.User.Identity,
                uploadViewModel.AddToProgress);
            return this.GetResult(model, true);
        }

        /// <summary>
        /// Show the calculator for an existing upload.
        /// </summary>
        /// <param name="uploadId">Id of the upload to view</param>
        /// <returns>The calculator view</returns>
        public ActionResult View(int? uploadId)
        {
            CalculatorViewModel model = uploadId.HasValue
                ? new CalculatorViewModel(
                    this.databaseCommandFactory,
                    this.userSettingsProvider,
                    this.gameData,
                    this.telemetryClient,
                    uploadId.Value,
                    this.User)
                : null;
            return this.GetResult(model, false);
        }

        private ActionResult GetResult(CalculatorViewModel model, bool wasDataPosted)
        {
            string errorMessage = null;
            if (model == null || !model.IsValid)
            {
                if (wasDataPosted)
                {
                    errorMessage = "The uploaded save was not valid";
                }
                else
                {
                    errorMessage = "The upload does not exist";
                }
            }
            else if (!model.IsPermitted)
            {
                errorMessage = "This upload belongs to a user with private uploads";
            }

            if (errorMessage != null)
            {
                this.ViewBag.ErrorMessage = errorMessage;
                return this.View("Error");
            }

            return this.View("Calculator", model);
        }
    }
}