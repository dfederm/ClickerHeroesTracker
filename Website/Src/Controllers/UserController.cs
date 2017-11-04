// <copyright file="UserController.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace Website.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using ClickerHeroesTrackerWebsite.Models;
    using ClickerHeroesTrackerWebsite.Models.Api.Users;
    using ClickerHeroesTrackerWebsite.Models.Game;
    using ClickerHeroesTrackerWebsite.Models.Settings;
    using ClickerHeroesTrackerWebsite.Services.Database;
    using ClickerHeroesTrackerWebsite.Services.Email;
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

        private readonly IEmailSender emailSender;

        public UserController(
            GameData gameData,
            TelemetryClient telemetryClient,
            IDatabaseCommandFactory databaseCommandFactory,
            IUserSettingsProvider userSettingsProvider,
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender)
        {
            this.gameData = gameData;
            this.telemetryClient = telemetryClient;
            this.databaseCommandFactory = databaseCommandFactory;
            this.userSettingsProvider = userSettingsProvider;
            this.userManager = userManager;
            this.emailSender = emailSender;
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
                && !userSettings.AreUploadsPublic.GetValueOrDefault(true)
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

        [Route("{userName}/settings")]
        [HttpGet]
        public async Task<IActionResult> GetSettings(string userName)
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

            var userSettings = this.userSettingsProvider.Get(userId);

            return this.Ok(userSettings);
        }

        [Route("{userName}/settings")]
        [HttpPatch]
        public async Task<IActionResult> PatchSettings(string userName, [FromBody] UserSettings model)
        {
            if (string.IsNullOrEmpty(userName))
            {
                return this.BadRequest();
            }

            if (model == null)
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

            this.userSettingsProvider.Patch(userId, model);

            return this.Ok();
        }

        [Route("{userName}/logins")]
        [HttpGet]
        public async Task<IActionResult> GetLogins(string userName)
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

            var model = new UserLogins
            {
                HasPassword = await this.userManager.HasPasswordAsync(user),
                ExternalLogins = (await this.userManager.GetLoginsAsync(user))
                    .Select(loginInfo => new ExternalLogin { ProviderName = loginInfo.LoginProvider, ExternalUserId = loginInfo.ProviderKey })
                    .ToList(),
            };

            return this.Ok(model);
        }

        [Route("{userName}/logins")]
        [HttpDelete]
        public async Task<IActionResult> RemoveLogin(string userName, [FromBody] ExternalLogin model)
        {
            if (string.IsNullOrEmpty(userName))
            {
                return this.BadRequest();
            }

            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
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

            var result = await this.userManager.RemoveLoginAsync(user, model.ProviderName, model.ExternalUserId);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    this.ModelState.AddModelError(string.Empty, error.Description);
                }

                return this.BadRequest(this.ModelState);
            }

            return this.Ok();
        }

        [Route("{userName}/setpassword")]
        [HttpPost]
        public async Task<IActionResult> SetPassword(string userName, [FromBody] SetPasswordRequest model)
        {
            if (string.IsNullOrEmpty(userName))
            {
                return this.BadRequest();
            }

            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
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

            var result = await this.userManager.AddPasswordAsync(user, model.NewPassword);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    this.ModelState.AddModelError(string.Empty, error.Description);
                }

                return this.BadRequest(this.ModelState);
            }

            return this.Ok();
        }

        [Route("{userName}/changepassword")]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(string userName, [FromBody] ChangePasswordRequest model)
        {
            if (string.IsNullOrEmpty(userName))
            {
                return this.BadRequest();
            }

            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
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

            var result = await this.userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    this.ModelState.AddModelError(string.Empty, error.Description);
                }

                return this.BadRequest(this.ModelState);
            }

            return this.Ok();
        }

        [Route("resetpassword")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            // Using email address since the username is public information
            var user = await this.userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return this.Ok();
            }

            var code = await this.userManager.GeneratePasswordResetTokenAsync(user);
            await this.emailSender.SendEmailAsync(
                model.Email,
                "Password Reset",
                $"There was a request to reset your Clicker Heroes Tracker password. If this was not you, please ignore this email.<br /><br />To reset your password, please enter this verification code:<br /><br />{code}");

            return this.Ok();
        }

        [Route("resetpasswordconfirmation")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPasswordConfirmation([FromBody] ResetPasswordConfirmationRequest model)
        {
            if (this.ModelState.IsValid)
            {
                // Using email address since the username is public information
                var user = await this.userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    // Don't reveal that the user does not exist
                    return this.Ok();
                }

                var result = await this.userManager.ResetPasswordAsync(user, model.Code, model.Password);
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
    }
}
