// <copyright file="AdminController.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using ClickerHeroesTrackerWebsite.Models;
    using ClickerHeroesTrackerWebsite.Services.Database;
    using ClickerHeroesTrackerWebsite.Services.UploadProcessing;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// The Admin controller is where Admin users can manage the site.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IDatabaseCommandFactory databaseCommandFactory;

        private readonly IUploadScheduler uploadScheduler;

        private readonly UserManager<ApplicationUser> userManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminController"/> class.
        /// </summary>
        /// <param name="databaseCommandFactory">The factory to create database commands.</param>
        /// <param name="uploadScheduler">The upload scheduler.</param>
        /// <param name="userManager">The user manager.</param>
        public AdminController(
            IDatabaseCommandFactory databaseCommandFactory,
            IUploadScheduler uploadScheduler,
            UserManager<ApplicationUser> userManager)
        {
            this.databaseCommandFactory = databaseCommandFactory;
            this.uploadScheduler = uploadScheduler;
            this.userManager = userManager;
        }

        /// <summary>
        /// GET: Admin
        /// </summary>
        /// <returns>The admin homepage view</returns>
        public async Task<IActionResult> Index()
        {
            this.ViewBag.Queues = await this.uploadScheduler.RetrieveQueueStatsAsync();

            // Be explicit about the view name since other actions directly call this action
            return this.View("Index");
        }

        /// <summary>
        /// GET: UpdateComputedStats
        /// </summary>
        /// <param name="uploadIds">The upload ids to recomute stats for</param>
        /// <param name="priority">The priority of the queue.</param>
        /// <returns>The admin homepage view</returns>
        public async Task<IActionResult> UpdateComputedStats(string uploadIds, UploadProcessingMessagePriority? priority)
        {
            if (!priority.HasValue)
            {
                this.ViewBag.Error = "Invalid Priority";
                return await this.Index();
            }

            var userId = this.userManager.GetUserId(this.User);

            var parsedUploadIds = new List<int>();
            if (uploadIds != null)
            {
                var uploadIdsRaw = uploadIds.Split(',');
                foreach (var uploadIdRaw in uploadIdsRaw)
                {
                    int uploadId;
                    if (int.TryParse(uploadIdRaw.Trim(), out uploadId))
                    {
                        parsedUploadIds.Add(uploadId);
                    }
                }
            }

            if (parsedUploadIds == null || parsedUploadIds.Count == 0)
            {
                this.ViewBag.Error = "No valid upload ids";
                return await this.Index();
            }

            var messages = parsedUploadIds.Select(uploadId => new UploadProcessingMessage { UploadId = uploadId, Requester = userId, Priority = priority.Value });
            await this.uploadScheduler.ScheduleAsync(messages);

            this.ViewBag.Message = $"Scheduled {parsedUploadIds.Count} uploads";
            return await this.Index();
        }

        /// <summary>
        /// Clears a upload processing queue
        /// </summary>
        /// <param name="priority">The priority of the queue to clear</param>
        /// <returns>The admin homepage view</returns>
        public async Task<IActionResult> ClearQueue(UploadProcessingMessagePriority? priority)
        {
            if (!priority.HasValue)
            {
                this.ViewBag.Error = "Invalid Priority";
                return await this.Index();
            }

            var numMessages = await this.uploadScheduler.ClearQueueAsync(priority.Value);

            this.ViewBag.Message = $"Cleared the {priority.Value} priority queue ({numMessages} messages)";
            return await this.Index();
        }

        public async Task<IActionResult> GetStaleAnonymousUploads()
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

            this.ViewBag.StaleUploads = uploadIds;
            this.ViewBag.Message = $"Found {uploadIds.Count} stale anonymous uploads";

            return await this.Index();
        }
    }
}