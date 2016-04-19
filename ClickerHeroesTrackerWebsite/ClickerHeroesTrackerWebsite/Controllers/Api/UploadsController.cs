// <copyright file="UploadsController.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Controllers.Api
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Security.Claims;
    using Database;
    using Instrumentation;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNet.Mvc;
    using Microsoft.AspNet.Authorization;
    using Models.Api;
    using Models.Api.Uploads;
    using Models.Calculator;
    using Models.Game;
    using Models.SaveData;
    using Models.Settings;
    using Utility;

    /// <summary>
    /// This controller handles the set of APIs that manage uploads
    /// </summary>
    [Route("api/uploads")]
    public sealed class UploadsController : Controller
    {
        private readonly IDatabaseCommandFactory databaseCommandFactory;

        private readonly GameData gameData;

        private readonly IUserSettingsProvider userSettingsProvider;

        private readonly TelemetryClient telemetryClient;

        private readonly ICounterProvider counterProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="UploadsController"/> class.
        /// </summary>
        public UploadsController(
            IDatabaseCommandFactory databaseCommandFactory,
            GameData gameData,
            IUserSettingsProvider userSettingsProvider,
            TelemetryClient telemetryClient,
            ICounterProvider counterProvider)
        {
            this.databaseCommandFactory = databaseCommandFactory;
            this.gameData = gameData;
            this.userSettingsProvider = userSettingsProvider;
            this.telemetryClient = telemetryClient;
            this.counterProvider = counterProvider;
        }

        /// <summary>
        /// Gets the user's uploads
        /// </summary>
        /// <param name="page">The page of results to get</param>
        /// <param name="count">The number of results per page</param>
        /// <returns>A response with the schema <see cref="UploadSummaryListResponse"/></returns>
        [Route("")]
        [HttpGet]
        [Authorize]
        public IActionResult List(
            int page = ParameterConstants.UploadSummaryList.Page.Default,
            int count = ParameterConstants.UploadSummaryList.Count.Default)
        {
            // Validate parameters
            if (page < ParameterConstants.UploadSummaryList.Page.Min)
            {
                return this.HttpBadRequest("Invalid parameter: page");
            }

            if (count < ParameterConstants.UploadSummaryList.Count.Min
                || count > ParameterConstants.UploadSummaryList.Count.Max)
            {
                return this.HttpBadRequest("Invalid parameter: count");
            }

            var userId = this.User.GetUserId();
            var model = new UploadSummaryListResponse()
            {
                Uploads = this.FetchUploads(userId, page, count),
                Pagination = this.FetchPagination(userId, page, count),
            };

            return this.Ok(model);
        }

        /// <summary>
        /// Retrieve the details for an upload.
        /// </summary>
        /// <remarks>BUGBUG 43 - Not implemented</remarks>
        /// <param name="id">The upload id</param>
        /// <returns>Empty 200, as this is not implemented yet</returns>
        [Route("{id:int}")]
        [HttpGet]
        public IActionResult Details(int id)
        {
            return this.Ok();
        }

        /// <summary>
        /// Add and an upload.
        /// </summary>
        /// <param name="rawUpload">The upload data</param>
        /// <returns>Empty 200, as this is not implemented yet</returns>
        [Route("")]
        [HttpPost]
        public IActionResult Add(RawUpload rawUpload)
        {
            if (rawUpload.EncodedSaveData == null)
            {
                // Not a valid save
                return this.HttpBadRequest();
            }

            // Instrument the encoded save data in case something goes wrong.
            // It needs to be spit into chunks as TelemetryClient has a max property value length.
            // See: https://azure.microsoft.com/en-us/documentation/articles/app-insights-pricing/#limits-summary
            var chunks = rawUpload.EncodedSaveData.SplitIntoChunks(10000);
            var properties = new Dictionary<string, string>();
            for (int i = 0; i < chunks.Length; i++)
            {
                properties.Add("EncodedSaveData_" + i, chunks[i]);
            }

            this.telemetryClient.TrackTrace("Upload", properties);

            // Only associate it with the user if they requested that it be added to their progress.
            var userId = rawUpload.AddToProgress && this.User.Identity.IsAuthenticated
                ? this.User.GetUserId()
                : null;

            var savedGame = SavedGame.Parse(rawUpload.EncodedSaveData);
            if (savedGame == null)
            {
                // Not a valid save
                return this.HttpBadRequest();
            }

            var userSettings = this.userSettingsProvider.Get(userId);

            var ancientLevels = new AncientLevelSummaryViewModel(
                this.gameData,
                savedGame,
                this.telemetryClient);
            var computedStats = new ComputedStatsViewModel(
                this.gameData,
                savedGame,
                userSettings,
                this.counterProvider);

            int uploadId;
            using (var command = this.databaseCommandFactory.Create())
            {
                command.BeginTransaction();

                // Insert Upload
                command.CommandText = @"
	                INSERT INTO Uploads(UserId, UploadContent)
                    VALUES(@UserId, @UploadContent);
                    SELECT SCOPE_IDENTITY();";
                command.Parameters = new Dictionary<string, object>
                {
                    { "@UserId", userId },
                    { "@UploadContent", rawUpload.EncodedSaveData },
                };
                uploadId = Convert.ToInt32(command.ExecuteScalar());

                // Insert computed stats
                command.CommandText = @"
                    INSERT INTO ComputedStats(UploadId, OptimalLevel, SoulsPerHour, SoulsPerAscension, AscensionTime, TitanDamage, SoulsSpent)
                    VALUES(@UploadId, @OptimalLevel, @SoulsPerHour, @SoulsPerAscension, @AscensionTime, @TitanDamage, @SoulsSpent);";
                command.Parameters = new Dictionary<string, object>
                {
                    { "@UploadId", uploadId },
                    { "@OptimalLevel", computedStats.OptimalLevel },
                    { "@SoulsPerHour", computedStats.SoulsPerHour },
                    { "@SoulsPerAscension", computedStats.OptimalSoulsPerAscension },
                    { "@AscensionTime", computedStats.OptimalAscensionTime },
                    { "@TitanDamage", computedStats.TitanDamage },
                    { "@SoulsSpent", computedStats.SoulsSpent },
                };
                command.ExecuteNonQuery();

                // Insert ancient levels
                foreach (var pair in ancientLevels.AncientLevels)
                {
                    command.CommandText = @"
                        INSERT INTO AncientLevels(UploadId, AncientId, Level)
                        VALUES(@UploadId, @AncientId, @Level);";
                    command.Parameters = new Dictionary<string, object>
                    {
                        { "@UploadId", uploadId },
                        { "@AncientId", pair.Key.Id },
                        { "@Level", pair.Value.AncientLevel },
                    };
                    command.ExecuteNonQuery();
                }

                var commited = command.CommitTransaction();
                if (!commited)
                {
                    return new HttpStatusCodeResult((int)HttpStatusCode.InternalServerError);
                }
            }

            return this.Ok(uploadId);
        }

        private List<UploadSummary> FetchUploads(string userId, int page, int count)
        {
            const string CommandText = @"
	            SELECT Id, UploadTime
	            FROM Uploads
	            WHERE UserId = @UserId
	            ORDER BY UploadTime DESC
		            OFFSET @Offset ROWS
		            FETCH NEXT @Count ROWS ONLY;";
            var parameters = new Dictionary<string, object>
            {
                { "@UserId", userId },
                { "@Offset", (page - 1) * count },
                { "@Count", count },
            };

            using (var command = this.databaseCommandFactory.Create(CommandText, parameters))
            using (var reader = command.ExecuteReader())
            {
                var uploads = new List<UploadSummary>(count);
                while (reader.Read())
                {
                    uploads.Add(new UploadSummary
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        TimeSubmitted = Convert.ToDateTime(reader["UploadTime"])
                    });
                }

                return uploads;
            }
        }

        private PaginationMetadata FetchPagination(string userId, int page, int count)
        {
            const string GetUploadCountCommandText = @"
	            SELECT COUNT(*) AS TotalUploads
		        FROM Uploads
		        WHERE UserId = @UserId";
            var parameters = new Dictionary<string, object>
            {
                { "@UserId", userId },
            };

            using (var command = this.databaseCommandFactory.Create(GetUploadCountCommandText, parameters))
            using (var reader = command.ExecuteReader())
            {
                if (!reader.Read())
                {
                    return null;
                }

                var pagination = new PaginationMetadata
                {
                    Count = Convert.ToInt32(reader["TotalUploads"])
                };

                var currentPath = this.Request.Path;
                if (page > 1)
                {
                    pagination.Previous = string.Format(
                        "{0}?{1}={2}&{3}={4}",
                        currentPath,
                        nameof(page),
                        page - 1,
                        nameof(count),
                        count);
                }

                if (page <= Math.Ceiling((float)pagination.Count / count))
                {
                    pagination.Next = string.Format(
                        "{0}?{1}={2}&{3}={4}",
                        currentPath,
                        nameof(page),
                        page + 1,
                        nameof(count),
                        count);
                }

                return pagination;
            }
        }

        internal static class ParameterConstants
        {
            internal static class UploadSummaryList
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