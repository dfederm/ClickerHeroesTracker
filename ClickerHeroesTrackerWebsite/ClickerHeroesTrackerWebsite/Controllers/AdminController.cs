// <copyright file="AdminController.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using ClickerHeroesTrackerWebsite.Services.Database;
    using ClickerHeroesTrackerWebsite.Services.UploadProcessing;
    using Microsoft.AspNet.Mvc;
    using Microsoft.AspNet.Authorization;
    using Microsoft.Extensions.OptionsModel;
    using Microsoft.ServiceBus.Messaging;

    /// <summary>
    /// The Admin controller is where Admin users can manage the site.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IDatabaseCommandFactory databaseCommandFactory;

        private readonly IUploadScheduler uploadScheduler;

        private readonly UploadProcessingSettings uploadProcessingSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminController"/> class.
        /// </summary>
        public AdminController(
            IDatabaseCommandFactory databaseCommandFactory,
            IUploadScheduler uploadScheduler,
            IOptions<UploadProcessingSettings> uploadProcessingSettingsOptions)
        {
            this.databaseCommandFactory = databaseCommandFactory;
            this.uploadScheduler = uploadScheduler;
            this.uploadProcessingSettings = uploadProcessingSettingsOptions.Value;
        }

        /// <summary>
        /// GET: Admin
        /// </summary>
        /// <returns>The admin homepage view</returns>
        public ActionResult Index()
        {
            return this.View();
        }

        /// <summary>
        /// GET: UpdateComputedStats
        /// </summary>
        /// <param name="uploadIds">The upload ids to recomute stats for</param>
        /// <returns>The admin homepage view</returns>
        public async Task<ActionResult> UpdateComputedStats(string uploadIds)
        {
            var userId = this.User.GetUserId();

            IList<int> parsedUploadIds = null;
            if (uploadIds != null)
            {
                if (uploadIds.Equals("ALL", StringComparison.OrdinalIgnoreCase))
                {
                    parsedUploadIds = new List<int>();

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
                return this.View("Index");
            }

            var messages = parsedUploadIds.Select(uploadId => new UploadProcessingMessage { UploadId = uploadId, Requester = userId, Priority = UploadProcessingMessagePriority.Low });
            await this.uploadScheduler.Schedule(messages);

            this.ViewBag.Message = $"Scheduled {parsedUploadIds.Count} uploads";
            return this.View("Index");
        }

        /// <summary>
        /// Clears a upload processing queue
        /// </summary>
        /// <param name="priority">The priority of the queue to clear</param>
        /// <returns>The admin homepage view</returns>
        public async Task<ActionResult> ClearQueue(UploadProcessingMessagePriority? priority)
        {
            if (!priority.HasValue)
            {
                this.ViewBag.Error = "Invalid Priority";
                return this.View("Index");
            }

            var connectionString = this.uploadProcessingSettings.ConnectionString;
            var client = QueueClient.CreateFromConnectionString(connectionString, $"UploadProcessing-{priority.Value}Priority");
            const int BatchSize = 1024;

            int messages = 0;
            while (client.Peek() != null)
            {
                // Batch the receive operation
                var oldMessages = client.ReceiveBatch(BatchSize);

                // Complete the messages
                var completeTasks = oldMessages.Select(m => m.CompleteAsync()).ToArray();

                messages += completeTasks.Length;

                // Wait for the tasks to complete.
                await Task.WhenAll(completeTasks);
            }

            this.ViewBag.Message = $"Cleared the {priority.Value} priority queue ({messages} messages)";
            return this.View("Index");
        }
    }
}