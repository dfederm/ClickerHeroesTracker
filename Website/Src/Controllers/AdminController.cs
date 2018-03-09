// <copyright file="AdminController.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace Website.Controllers
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
    using Website.Services.UploadProcessing;

    [Route("api/admin")]
    [Authorize(Roles = "Admin", AuthenticationSchemes = OAuthValidationDefaults.AuthenticationScheme)]
    [ApiController]
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
        public async Task<ActionResult<List<UploadQueueStats>>> Queues() => (await this.uploadScheduler.RetrieveQueueStatsAsync()).ToList();

        [Route("recompute")]
        [HttpPost]
        public Task Recompute(RecomputeRequest model)
        {
            var userId = this.userManager.GetUserId(this.User);
            var messages = model.UploadIds.Select(uploadId => new UploadProcessingMessage { UploadId = uploadId, Requester = userId, Priority = model.Priority });
            return this.uploadScheduler.ScheduleAsync(messages);
        }

        [Route("clearqueue")]
        [HttpPost]
        public async Task<ActionResult<int>> ClearQueue(ClearQueueRequest model) => await this.uploadScheduler.ClearQueueAsync(model.Priority);

        [Route("staleuploads")]
        [HttpGet]
        public async Task<ActionResult<List<int>>> StaleUploads()
        {
            const string CommandText = @"
                SELECT Id
                FROM Uploads
                WHERE UserId IS NULL
                AND UploadTime < DATEADD(day, -30, GETDATE())";
            var uploadIds = new List<int>();
            using (var command = this.databaseCommandFactory.Create(CommandText))
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    var uploadId = Convert.ToInt32(reader["Id"]);
                    uploadIds.Add(uploadId);
                }
            }

            return uploadIds;
        }

        [Route("countinvalidauthtokens")]
        [HttpGet]
        public async Task<ActionResult<int>> CountInvalidAuthTokens()
        {
            const string CommandText = @"
                SELECT COUNT(*) AS Count
                FROM OpenIddictTokens
                WHERE ExpirationDate < CAST(GETUTCDATE() AS datetimeoffset)
                OR Status <> 'valid'
                OR Status IS NULL";
            using (var command = this.databaseCommandFactory.Create(CommandText))
            {
                return Convert.ToInt32(await command.ExecuteScalarAsync());
            }
        }

        [Route("pruneinvalidauthtokens")]
        [HttpPost]
        public async Task PruneInvalidAuthTokens(PruneInvalidAuthTokensRequest model)
        {
            const string CommandTextFormat = @"
                DELETE TOP (@BatchSize)
                FROM OpenIddictTokens
                WHERE ExpirationDate < CAST(GETUTCDATE() AS datetimeoffset)
                OR Status <> 'valid'
                OR Status IS NULL";
            var parameters = new Dictionary<string, object>
            {
                { "BatchSize", model.BatchSize },
            };

            using (var command = this.databaseCommandFactory.Create(CommandTextFormat, parameters))
            {
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}