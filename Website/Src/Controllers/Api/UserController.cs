// <copyright file="UserController.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace Website.Controllers.Api
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
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
    using Website.Models.Api.Users;

    [Route("api/users")]
    [Authorize]
    public class UserController : Controller
    {
        private static Regex userNameRegex = new Regex("\\w+", RegexOptions.Compiled);

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

        [Route("")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest createUser)
        {
            if (this.ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = createUser.UserName, Email = createUser.Email };
                var result = await this.userManager.CreateAsync(user, createUser.Password);
                if (result.Succeeded)
                {
                    return this.Ok();
                }

                foreach (var error in result.Errors)
                {
                    this.ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return this.BadRequest(this.ModelState);
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

        [Route("{userName}/follows")]
        [HttpGet]
        public async Task<IActionResult> Follows(string userName)
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

            var currentUserId = this.userManager.GetUserId(this.User);
            if (!userId.Equals(currentUserId, StringComparison.OrdinalIgnoreCase)
                && !this.User.IsInRole("Admin"))
            {
                return this.Forbid();
            }

            var follows = new List<string>();
            var parameters = new Dictionary<string, object>
            {
                { "@UserId", userId },
            };
            const string GetUserFollowsCommandText = @"
	            SELECT UserName
	            FROM UserFollows
	            INNER JOIN AspNetUsers
	            ON UserFollows.FollowUserId = AspNetUsers.Id
	            WHERE UserId = @UserId
	            ORDER BY UserName ASC";
            using (var command = this.databaseCommandFactory.Create(
                GetUserFollowsCommandText,
                parameters))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    follows.Add(reader["UserName"].ToString());
                }
            }

            var data = new FollowsData
            {
                Follows = follows,
            };

            return this.Ok(data);
        }
    }
}
