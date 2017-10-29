// <copyright file="AdminController.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace Website.Controllers.Api
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AspNet.Security.OAuth.Validation;
    using ClickerHeroesTrackerWebsite.Models;
    using ClickerHeroesTrackerWebsite.Services.Database;
    using ClickerHeroesTrackerWebsite.Services.UploadProcessing;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Website.Models.Api.Admin;

    [Route("api/admin")]
    [Authorize(Roles = "Admin", AuthenticationSchemes = OAuthValidationDefaults.AuthenticationScheme)]
    public class AdminController : Controller
    {
        private readonly IDatabaseCommandFactory databaseCommandFactory;

        private readonly IUploadScheduler uploadScheduler;

        private readonly UserManager<ApplicationUser> userManager;

        public AdminController(
            IDatabaseCommandFactory databaseCommandFactory,
            IUploadScheduler uploadScheduler,
            UserManager<ApplicationUser> userManager)
        {
            this.databaseCommandFactory = databaseCommandFactory;
            this.uploadScheduler = uploadScheduler;
            this.userManager = userManager;
        }

        [Route("queues")]
        [HttpGet]
        public async Task<IActionResult> Queues()
        {
            var queues = await this.uploadScheduler.RetrieveQueueStatsAsync();
            return this.Ok(queues);
        }

        [Route("recompute")]
        [HttpPost]
        public async Task<IActionResult> Recompute([FromBody] RecomputeRequest model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            if (model.UploadIds == null || model.UploadIds.Count == 0)
            {
                return this.BadRequest();
            }

            var userId = this.userManager.GetUserId(this.User);
            var messages = model.UploadIds.Select(uploadId => new UploadProcessingMessage { UploadId = uploadId, Requester = userId, Priority = model.Priority });
            await this.uploadScheduler.ScheduleAsync(messages);

            return this.Ok();
        }

        [Route("clearqueue")]
        [HttpPost]
        public async Task<IActionResult> ClearQueue([FromBody] ClearQueueRequest model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var numMessages = await this.uploadScheduler.ClearQueueAsync(model.Priority);
            return this.Ok(numMessages);
        }

        [Route("staleuploads")]
        [HttpGet]
        public IActionResult StaleUploads()
        {
            const string CommandText = @"
                SELECT Id
                FROM Uploads
                WHERE UserId IS NULL
                AND UploadTime < DATEADD(day, -30, GETDATE())";
            var uploadIds = new List<int>();
            using (var command = this.databaseCommandFactory.Create(CommandText))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var uploadId = Convert.ToInt32(reader["Id"]);
                    uploadIds.Add(uploadId);
                }
            }

            return this.Ok(uploadIds);
        }
    }
}