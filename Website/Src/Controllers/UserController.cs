﻿// <copyright file="UserController.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace Website.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using ClickerHeroesTrackerWebsite.Models;
    using ClickerHeroesTrackerWebsite.Models.Api;
    using ClickerHeroesTrackerWebsite.Models.Api.Uploads;
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
    using Website.Services.Clans;

    [Route("api/users")]
    [Authorize]
    [ApiController]
    public class UserController : Controller
    {
        private readonly GameData gameData;

        private readonly TelemetryClient telemetryClient;

        private readonly IDatabaseCommandFactory databaseCommandFactory;

        private readonly IUserSettingsProvider userSettingsProvider;

        private readonly UserManager<ApplicationUser> userManager;

        private readonly IEmailSender emailSender;

        private readonly IClanManager clanManager;

        public UserController(
            GameData gameData,
            TelemetryClient telemetryClient,
            IDatabaseCommandFactory databaseCommandFactory,
            IUserSettingsProvider userSettingsProvider,
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender,
            IClanManager clanManager)
        {
            this.gameData = gameData;
            this.telemetryClient = telemetryClient;
            this.databaseCommandFactory = databaseCommandFactory;
            this.userSettingsProvider = userSettingsProvider;
            this.userManager = userManager;
            this.emailSender = emailSender;
            this.clanManager = clanManager;
        }

        [Route("")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Create(CreateUserRequest createUser)
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

            return this.BadRequest(this.ModelState);
        }

        [Route("{userName}")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<User>> Get(string userName)
        {
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

            return new User
            {
                Name = user.UserName,
                ClanName = await this.clanManager.GetClanNameAsync(userId),
            };
        }

        [Route("{userName}/uploads")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<UploadSummaryListResponse>> Uploads(
            string userName,
            int page = ParameterConstants.Uploads.Page.Default,
            int count = ParameterConstants.Uploads.Count.Default)
        {
            // Validate parameters
            if (page < ParameterConstants.Uploads.Page.Min)
            {
                return this.BadRequest();
            }

            if (count < ParameterConstants.Uploads.Count.Min
                || count > ParameterConstants.Uploads.Count.Max)
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

            const string GetUploadsCommandText = @"
                SELECT Id,
                    UploadTime,
                    AscensionsLifetime AS AscensionNumber,
                    HighestZoneThisTranscension AS Zone,
                    RTRIM(SoulsSpent) AS Souls
                FROM Uploads
                INNER JOIN ComputedStats
                ON ComputedStats.UploadId = Uploads.Id
                WHERE UserId = @UserId
                ORDER BY UploadTime DESC
                OFFSET @Offset ROWS
                FETCH NEXT @Count ROWS ONLY;";
            var getUploadsCommandparameters = new Dictionary<string, object>
            {
                { "@UserId", userId },
                { "@Offset", (page - 1) * count },
                { "@Count", count },
            };

            var uploads = new List<UploadSummary>(count);
            using (var command = this.databaseCommandFactory.Create(GetUploadsCommandText, getUploadsCommandparameters))
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    uploads.Add(new UploadSummary
                    {
                        Id = Convert.ToInt32(reader["Id"]),

                        // The DateTime is a datetime2 which has no timezone so comes out as DateTimeKind.Unknown. Se need to specify the kind so it gets serialized correctly.
                        TimeSubmitted = DateTime.SpecifyKind(Convert.ToDateTime(reader["UploadTime"]), DateTimeKind.Utc),

                        AscensionNumber = Convert.ToInt32(reader["AscensionNumber"]),
                        Zone = Convert.ToInt32(reader["Zone"]),
                        Souls = reader["Souls"].ToString(),
                    });
                }
            }

            const string GetUploadCountCommandText = @"
                SELECT COUNT(*) AS TotalUploads
                FROM Uploads
                WHERE UserId = @UserId";
            var getUploadCountparameters = new Dictionary<string, object>
            {
                { "@UserId", userId },
            };

            var pagination = new PaginationMetadata();
            using (var command = this.databaseCommandFactory.Create(GetUploadCountCommandText, getUploadCountparameters))
            using (var reader = await command.ExecuteReaderAsync())
            {
                if (reader.Read())
                {
                    pagination.Count = Convert.ToInt32(reader["TotalUploads"]);

                    var currentPath = this.Request.Path;
                    if (page > 1)
                    {
                        pagination.Previous = $"{currentPath}?{nameof(page)}={page - 1}&{nameof(count)}={count}";
                    }

                    if (page <= Math.Ceiling((float)pagination.Count / count))
                    {
                        pagination.Next = $"{currentPath}?{nameof(page)}={page + 1}&{nameof(count)}={count}";
                    }
                }
            }

            return new UploadSummaryListResponse
            {
                Uploads = uploads,
                Pagination = pagination,
            };
        }

        [Route("{userName}/progress")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ProgressData>> Progress(
            string userName,
            DateTime? start,
            DateTime? end)
        {
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

            var data = await ProgressData.CreateAsync(
                this.gameData,
                this.telemetryClient,
                this.databaseCommandFactory,
                userId,
                startDate,
                endDate);

            if (data == null)
            {
                return this.StatusCode(500);
            }

            return data;
        }

        [Route("{userName}/follows")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<FollowsData>> Follows(string userName)
        {
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
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    follows.Add(reader["UserName"].ToString());
                }
            }

            return new FollowsData
            {
                Follows = follows,
            };
        }

        [Route("{userName}/follows")]
        [HttpPost]
        public async Task<ActionResult> AddFollow(string userName, AddFollowRequest model)
        {
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

            var followUser = await this.userManager.FindByNameAsync(model.FollowUserName);
            if (followUser == null)
            {
                return this.NotFound();
            }

            var followUserId = await this.userManager.GetUserIdAsync(followUser);
            if (string.IsNullOrEmpty(followUserId))
            {
                return this.NotFound();
            }

            var parameters = new Dictionary<string, object>
            {
                { "@UserId", userId },
                { "@FollowUserId", followUserId },
            };
            const string CommandText = @"
                MERGE INTO UserFollows WITH (HOLDLOCK)
                USING
                    (VALUES (@UserId, @FollowUserId))
                        AS Input(UserId, FollowUserId)
                    ON UserFollows.UserId = Input.UserId
                    AND UserFollows.FollowUserId = Input.FollowUserId
                WHEN NOT MATCHED THEN
                    INSERT (UserId, FollowUserId)
                    VALUES(@UserId, @FollowUserId);";
            using (var command = this.databaseCommandFactory.Create(
                CommandText,
                parameters))
            {
                await command.ExecuteNonQueryAsync();
            }

            return this.Ok();
        }

        [Route("{userName}/follows/{followUserName}")]
        [HttpDelete]
        public async Task<ActionResult> RemoveFollow(string userName, string followUserName)
        {
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

            var followUser = await this.userManager.FindByNameAsync(followUserName);
            if (followUser == null)
            {
                return this.NotFound();
            }

            var followUserId = await this.userManager.GetUserIdAsync(followUser);
            if (string.IsNullOrEmpty(followUserId))
            {
                return this.NotFound();
            }

            var parameters = new Dictionary<string, object>
            {
                { "@UserId", userId },
                { "@FollowUserId", followUserId },
            };
            const string CommandText = @"
                DELETE
                FROM UserFollows
                WHERE UserId = @UserId
                AND FollowUserId = @FollowUserId;
                SELECT @@ROWCOUNT;";
            using (var command = this.databaseCommandFactory.Create(
                CommandText,
                parameters))
            {
                var numDeletions = Convert.ToInt32(await command.ExecuteScalarAsync());
                if (numDeletions == 0)
                {
                    return this.NotFound();
                }
            }

            return this.Ok();
        }

        [Route("{userName}/settings")]
        [HttpGet]
        public async Task<ActionResult<UserSettings>> GetSettings(string userName)
        {
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

            return await this.userSettingsProvider.GetAsync(userId);
        }

        [Route("{userName}/settings")]
        [HttpPatch]
        public async Task<ActionResult> PatchSettings(string userName, UserSettings model)
        {
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

            await this.userSettingsProvider.PatchAsync(userId, model);

            return this.Ok();
        }

        [Route("{userName}/logins")]
        [HttpGet]
        public async Task<ActionResult<UserLogins>> GetLogins(string userName)
        {
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

            return new UserLogins
            {
                HasPassword = await this.userManager.HasPasswordAsync(user),
                ExternalLogins = (await this.userManager.GetLoginsAsync(user))
                    .Select(loginInfo => new ExternalLogin { ProviderName = loginInfo.LoginProvider, ExternalUserId = loginInfo.ProviderKey })
                    .ToList(),
            };
        }

        [Route("{userName}/logins")]
        [HttpDelete]
        public async Task<ActionResult> RemoveLogin(string userName, ExternalLogin model)
        {
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
        public async Task<ActionResult> SetPassword(string userName, SetPasswordRequest model)
        {
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
        public async Task<ActionResult> ChangePassword(string userName, ChangePasswordRequest model)
        {
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
        public async Task<ActionResult> ResetPassword(ResetPasswordRequest model)
        {
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
        public async Task<ActionResult> ResetPasswordConfirmation(ResetPasswordConfirmationRequest model)
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

            return this.BadRequest(this.ModelState);
        }

        internal static class ParameterConstants
        {
            internal static class Uploads
            {
                internal static class Page
                {
                    internal const int Min = 1;

                    internal const int Default = 1;
                }

                internal static class Count
                {
                    internal const int Min = 1;

                    internal const int Max = 100;

                    internal const int Default = 10;
                }
            }
        }
    }
}
