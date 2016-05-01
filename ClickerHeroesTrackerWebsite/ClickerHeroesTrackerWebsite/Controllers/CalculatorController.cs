// <copyright file="CalculatorController.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Controllers
{
    using ClickerHeroesTrackerWebsite.Database;
    using ClickerHeroesTrackerWebsite.Models.Calculator;
    using ClickerHeroesTrackerWebsite.Models.Settings;
    using Microsoft.AspNet.Mvc;

    /// <summary>
    /// The calculator controller shows stats for a given upload.
    /// </summary>
    public class CalculatorController : Controller
    {
        private readonly IDatabaseCommandFactory databaseCommandFactory;

        private readonly IUserSettingsProvider userSettingsProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="CalculatorController"/> class.
        /// </summary>
        public CalculatorController(
            IDatabaseCommandFactory databaseCommandFactory,
            IUserSettingsProvider userSettingsProvider)
        {
            this.databaseCommandFactory = databaseCommandFactory;
            this.userSettingsProvider = userSettingsProvider;
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
                    uploadId.Value,
                    this.User)
                : null;

            string errorMessage = null;
            if (model == null || !model.IsValid)
            {
                errorMessage = "The upload does not exist";
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