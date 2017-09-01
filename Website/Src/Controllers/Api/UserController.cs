// <copyright file="UserController.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace Website.Controllers.Api
{
    using System;
    using System.Threading.Tasks;
    using ClickerHeroesTrackerWebsite.Models;
    using ClickerHeroesTrackerWebsite.Models.Api.Users;
    using ClickerHeroesTrackerWebsite.Models.Game;
    using ClickerHeroesTrackerWebsite.Models.Settings;
    using ClickerHeroesTrackerWebsite.Services.Database;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/users")]
    [Authorize]
    public class UserController : Controller
    {
        private readonly GameData gameData;

        private readonly TelemetryClient telemetryClient;

        private readonly IDatabaseCommandFactory databaseCommandFactory;

        private readonly IUserSettingsProvider userSettingsProvider;

        private readonly UserManager<ApplicationUser> userManager;

        public UserController(
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

        [Route("{userName}/progress")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Progress(
            string userName,
            [FromQuery] DateTime? start,
            [FromQuery] DateTime? end)
        {
            if (string.IsNullOrEmpty(userName))
            {
                return this.BadRequest();
            }

            var user = await this.userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return this.NotFound();
            }

            var userId = await this.userManager.GetUserIdAsync(user);
            if (string.IsNullOrEmpty(userId))
            {
                return this.NotFound();
            }

            var userSettings = this.userSettingsProvider.Get(userId);
            var currentUserId = this.userManager.GetUserId(this.User);
            if (!userId.Equals(currentUserId, StringComparison.OrdinalIgnoreCase)
                && !userSettings.AreUploadsPublic
                && !this.User.IsInRole("Admin"))
            {
                return this.Forbid();
            }

            var currentUserSettings = this.userSettingsProvider.Get(currentUserId);

            // Fill in missing range values as needed
            DateTime startDate;
            DateTime endDate;
            if (start.HasValue && end.HasValue)
            {
                startDate = start.Value;
                endDate = end.Value;
            }
            else if (start.HasValue)
            {
                // Default to a week after the start date
                startDate = start.Value;
                endDate = start.Value.AddDays(7);
            }
            else if (end.HasValue)
            {
                // Default to a week before the end date
                startDate = end.Value.AddDays(-7);
                endDate = end.Value;
            }
            else
            {
                // Default to the past week
                var now = DateTime.UtcNow;
                startDate = now.AddDays(-7);
                endDate = now;
            }

            var data = new ProgressData(
                this.gameData,
                this.telemetryClient,
                this.databaseCommandFactory,
                userId,
                startDate,
                endDate);

            if (!data.IsValid)
            {
                return this.StatusCode(500);
            }

            return this.Ok(data);
        }
    }
}
