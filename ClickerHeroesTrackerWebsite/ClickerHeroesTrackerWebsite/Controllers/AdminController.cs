// <copyright file="AdminController.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using ClickerHeroesTrackerWebsite.Models;
    using ClickerHeroesTrackerWebsite.Services.Database;
    using ClickerHeroesTrackerWebsite.Services.UploadProcessing;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Options;
    using Microsoft.WindowsAzure.Storage.Queue;

    /// <summary>
    /// The Admin controller is where Admin users can manage the site.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private static string[] priorities = Enum.GetNames(typeof(UploadProcessingMessagePriority));

        private readonly IDatabaseCommandFactory databaseCommandFactory;

        private readonly IUploadScheduler uploadScheduler;

        private readonly CloudQueueClient queueClient;

        private readonly UserManager<ApplicationUser> userManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminController"/> class.
        /// </summary>
        public AdminController(
            IDatabaseCommandFactory databaseCommandFactory,
            IUploadScheduler uploadScheduler,
            CloudQueueClient queueClient,
            UserManager<ApplicationUser> userManager)
        {
            this.databaseCommandFactory = databaseCommandFactory;
            this.uploadScheduler = uploadScheduler;
            this.queueClient = queueClient;
            this.userManager = userManager;
        }

        /// <summary>
        /// GET: Admin
        /// </summary>
        /// <returns>The admin homepage view</returns>
        public async Task<IActionResult> Index()
        {
            var queues = new Dictionary<string, int>();
            foreach (var priority in priorities)
            {
                var queue = queueClient.GetQueueReference($"upload-processing-{priority.ToLower()}-priority");
                await queue.FetchAttributesAsync();
                var numMessages = queue.ApproximateMessageCount.GetValueOrDefault();
                queues.Add(priority, numMessages);
            }

            this.ViewBag.Queues = queues;

            // Be explicit about the view name since other actions directly call this action
            return this.View("Index");
        }

        /// <summary>
        /// GET: UpdateComputedStats
        /// </summary>
        /// <param name="uploadIds">The upload ids to recomute stats for</param>
        /// <returns>The admin homepage view</returns>
        public async Task<IActionResult> UpdateComputedStats(string uploadIds)
        {
            var userId = this.userManager.GetUserId(this.User);

            var parsedUploadIds = new List<int>();
            if (uploadIds != null)
            {
                if (uploadIds.Equals("ALL", StringComparison.OrdinalIgnoreCase))
                {
                    const string CommandText = "SELECT Id FROM Uploads ORDER BY UploadTime DESC";
                    using (var command = this.databaseCommandFactory.Create(CommandText))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var uploadId = Convert.ToInt32(reader["Id"]);
                            parsedUploadIds.Add(uploadId);
                        }
                    }
                }
                else
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
            }

            if (parsedUploadIds == null || parsedUploadIds.Count == 0)
            {
                this.ViewBag.Error = "No valid upload ids";
                return await this.Index();
            }

            var messages = parsedUploadIds.Select(uploadId => new UploadProcessingMessage { UploadId = uploadId, Requester = userId, Priority = UploadProcessingMessagePriority.Low });
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

            var queue = queueClient.GetQueueReference($"upload-processing-{priority.Value.ToString().ToLower()}-priority");

            var numMessages = queue.ApproximateMessageCount;

            await queue.ClearAsync();

            this.ViewBag.Message = $"Cleared the {priority.Value} priority queue ({numMessages} messages)";
            return await this.Index();
        }
    }
}