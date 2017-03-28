// <copyright file="UploadsController.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Controllers.Api
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using ClickerHeroesTrackerWebsite.Instrumentation;
    using ClickerHeroesTrackerWebsite.Models;
    using ClickerHeroesTrackerWebsite.Models.Api;
    using ClickerHeroesTrackerWebsite.Models.Api.Stats;
    using ClickerHeroesTrackerWebsite.Models.Api.Uploads;
    using ClickerHeroesTrackerWebsite.Models.Game;
    using ClickerHeroesTrackerWebsite.Models.SaveData;
    using ClickerHeroesTrackerWebsite.Models.Settings;
    using ClickerHeroesTrackerWebsite.Models.Stats;
    using ClickerHeroesTrackerWebsite.Services.Database;
    using ClickerHeroesTrackerWebsite.Utility;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;

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

        private readonly UserManager<ApplicationUser> userManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="UploadsController"/> class.
        /// </summary>
        public UploadsController(
            IDatabaseCommandFactory databaseCommandFactory,
            GameData gameData,
            IUserSettingsProvider userSettingsProvider,
            TelemetryClient telemetryClient,
            ICounterProvider counterProvider,
            UserManager<ApplicationUser> userManager)
        {
            this.databaseCommandFactory = databaseCommandFactory;
            this.gameData = gameData;
            this.userSettingsProvider = userSettingsProvider;
            this.telemetryClient = telemetryClient;
            this.counterProvider = counterProvider;
            this.userManager = userManager;
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
                return this.BadRequest("Invalid parameter: page");
            }

            if (count < ParameterConstants.UploadSummaryList.Count.Min
                || count > ParameterConstants.UploadSummaryList.Count.Max)
            {
                return this.BadRequest("Invalid parameter: count");
            }

            var userId = this.userManager.GetUserId(this.User);
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
        /// <returns>A response with the schema <see cref="Upload"/></returns>
        [Route("{uploadId:int}")]
        [HttpGet]
        public IActionResult Details(int uploadId)
        {
            if (uploadId < 0)
            {
                return this.BadRequest();
            }

            var userId = this.userManager.GetUserId(this.User);
            var userSettings = this.userSettingsProvider.Get(userId);

            var uploadIdParameters = new Dictionary<string, object>
            {
                { "@UploadId", uploadId },
            };

            string uploadContent;
            PlayStyle playStyle;
            var upload = new Upload { Id = uploadId };
            const string GetUploadDataCommandText = @"
	            SELECT UserId, UserName, UploadTime, UploadContent, PlayStyle
                FROM Uploads
                LEFT JOIN AspNetUsers
                ON Uploads.UserId = AspNetUsers.Id
                WHERE Uploads.Id = @UploadId";
            using (var command = this.databaseCommandFactory.Create(
                GetUploadDataCommandText,
                uploadIdParameters))
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    var uploadUserId = reader["UserId"].ToString();
                    var uploadUserName = reader["UserName"].ToString();

                    if (!string.IsNullOrEmpty(uploadUserId))
                    {
                        upload.User = new User()
                        {
                            Id = uploadUserId,
                            Name = uploadUserName,
                        };
                    }

                    upload.TimeSubmitted = Convert.ToDateTime(reader["UploadTime"]);

                    uploadContent = reader["UploadContent"].ToString();
                    playStyle = reader["PlayStyle"].ToString().SafeParseEnum<PlayStyle>();
                }
                else
                {
                    // If we didn't get data, it's an upload that doesn't exist
                    return this.NotFound();
                }
            }

            var isAdmin = this.User.IsInRole("Admin");
            var isUploadAnonymous = upload.User == null;
            var isOwn = !isUploadAnonymous && string.Equals(userId, upload.User.Id, StringComparison.OrdinalIgnoreCase);
            var uploadUserSettings = isOwn ? userSettings : this.userSettingsProvider.Get(upload.User?.Id);
            var isPublic = isUploadAnonymous || uploadUserSettings.AreUploadsPublic;
            var isPermitted = isOwn || isPublic || isAdmin;

            if (!isPermitted)
            {
                return this.Unauthorized();
            }

            // Only return the raw upload content if it's the requesting user's or an admin requested it.
            if (isOwn || isAdmin)
            {
                upload.UploadContent = uploadContent;
            }

            // Set the play style.
            upload.PlayStyle = playStyle;

            var savedGame = SavedGame.Parse(uploadContent);
            upload.Stats = new Dictionary<StatType, double>();

            // Get ancient level stats
            var ancientLevelsModel = new AncientLevelsModel(
                this.gameData,
                savedGame,
                telemetryClient);
            foreach (var ancientLevelInfo in ancientLevelsModel.AncientLevels)
            {
                var ancientLevel = ancientLevelInfo.Value.AncientLevel;
                if (ancientLevel > 0)
                {
                    upload.Stats.Add(AncientIds.GetAncientStatType(ancientLevelInfo.Key), ancientLevel);
                }

                var itemLevel = ancientLevelInfo.Value.ItemLevel;
                if (itemLevel > 0)
                {
                    upload.Stats.Add(AncientIds.GetItemStatType(ancientLevelInfo.Key), itemLevel);
                }
            }

            // Get outsider level stats
            var outsiderLevelsModel = new OutsiderLevelsModel(
                this.gameData,
                savedGame,
                this.telemetryClient);
            foreach (var pair in outsiderLevelsModel.OutsiderLevels)
            {
                var outsiderLevel = pair.Value.Level;
                if (outsiderLevel > 0)
                {
                    upload.Stats.Add(OutsiderIds.GetOusiderStatType(pair.Key), outsiderLevel);
                }
            }

            // Get misc stats
            var miscellaneousStatsModel = new MiscellaneousStatsModel(
                this.gameData,
                savedGame);
            upload.Stats.Add(StatType.AscensionsLifetime, miscellaneousStatsModel.AscensionsLifetime);
            upload.Stats.Add(StatType.AscensionsThisTranscension, miscellaneousStatsModel.AscensionsThisTranscension);
            upload.Stats.Add(StatType.HeroSoulsSacrificed, miscellaneousStatsModel.HeroSoulsSacrificed);
            upload.Stats.Add(StatType.HeroSoulsSpent, miscellaneousStatsModel.HeroSoulsSpent);
            upload.Stats.Add(StatType.HighestZoneLifetime, miscellaneousStatsModel.HighestZoneLifetime);
            upload.Stats.Add(StatType.HighestZoneThisTranscension, miscellaneousStatsModel.HighestZoneThisTranscension);
            upload.Stats.Add(StatType.Rubies, miscellaneousStatsModel.Rubies);
            upload.Stats.Add(StatType.TitanDamage, miscellaneousStatsModel.TitanDamage);
            upload.Stats.Add(StatType.TotalAncientSouls, miscellaneousStatsModel.TotalAncientSouls);
            upload.Stats.Add(StatType.TranscendentPower, miscellaneousStatsModel.TranscendentPower);
            upload.Stats.Add(StatType.MaxTranscendentPrimalReward, miscellaneousStatsModel.MaxTranscendentPrimalReward);
            upload.Stats.Add(StatType.BossLevelToTranscendentPrimalCap, miscellaneousStatsModel.BossLevelToTranscendentPrimalCap);
            upload.Stats.Add(StatType.HeroSouls, miscellaneousStatsModel.HeroSouls);
            upload.Stats.Add(StatType.PendingSouls, miscellaneousStatsModel.PendingSouls);

            return this.Ok(upload);
        }

        /// <summary>
        /// Add and an upload.
        /// </summary>
        /// <param name="uploadRequest">The upload data</param>
        /// <returns>The new upload id</returns>
        [Route("")]
        [HttpPost]
        public IActionResult Add(UploadRequest uploadRequest)
        {
            if (uploadRequest.EncodedSaveData == null)
            {
                // Not a valid save
                return this.BadRequest();
            }

            // Instrument the encoded save data in case something goes wrong.
            // It needs to be spit into chunks as TelemetryClient has a max property value length.
            // See: https://azure.microsoft.com/en-us/documentation/articles/app-insights-pricing/#limits-summary
            var chunks = uploadRequest.EncodedSaveData.SplitIntoChunks(10000);
            var properties = new Dictionary<string, string>();
            for (int i = 0; i < chunks.Length; i++)
            {
                properties.Add("EncodedSaveData_" + i, chunks[i]);
            }

            this.telemetryClient.TrackTrace("Upload", properties);

            // Only associate it with the user if they requested that it be added to their progress.
            var userId = uploadRequest.AddToProgress && this.User.Identity.IsAuthenticated
                ? this.userManager.GetUserId(this.User)
                : null;

            var savedGame = SavedGame.Parse(uploadRequest.EncodedSaveData);
            if (savedGame == null)
            {
                // Not a valid save
                return this.BadRequest();
            }

            PlayStyle playStyle;
            if (uploadRequest.PlayStyle.HasValue)
            {
                playStyle = uploadRequest.PlayStyle.Value;
            }
            else
            {
                var userSettings = this.userSettingsProvider.Get(userId);
                playStyle = userSettings.PlayStyle;
            }

            var ancientLevels = new AncientLevelsModel(
                this.gameData,
                savedGame,
                this.telemetryClient);
            var outsiderLevels = new OutsiderLevelsModel(
                this.gameData,
                savedGame,
                this.telemetryClient);
            var miscellaneousStatsModel = new MiscellaneousStatsModel(
                this.gameData,
                savedGame);

            int uploadId;
            using (var command = this.databaseCommandFactory.Create())
            {
                command.BeginTransaction();

                // Insert Upload
                command.CommandText = @"
	                INSERT INTO Uploads(UserId, UploadContent, PlayStyle)
                    VALUES(@UserId, @UploadContent, @PlayStyle);
                    SELECT SCOPE_IDENTITY();";
                command.Parameters = new Dictionary<string, object>
                {
                    { "@UserId", userId },
                    { "@UploadContent", uploadRequest.EncodedSaveData },
                    { "@PlayStyle", playStyle.ToString() },
                };
                uploadId = Convert.ToInt32(command.ExecuteScalar());

                // Insert computed stats
                command.CommandText = @"
                    INSERT INTO ComputedStats(
                        UploadId,
                        TitanDamage,
                        SoulsSpent,
                        HeroSoulsSacrificed,
                        TotalAncientSouls,
                        TranscendentPower,
                        Rubies,
                        HighestZoneThisTranscension,
                        HighestZoneLifetime,
                        AscensionsThisTranscension,
                        AscensionsLifetime,
                        MaxTranscendentPrimalReward,
                        BossLevelToTranscendentPrimalCap)
                    VALUES(
                        @UploadId,
                        @TitanDamage,
                        @SoulsSpent,
                        @HeroSoulsSacrificed,
                        @TotalAncientSouls,
                        @TranscendentPower,
                        @Rubies,
                        @HighestZoneThisTranscension,
                        @HighestZoneLifetime,
                        @AscensionsThisTranscension,
                        @AscensionsLifetime,
                        @MaxTranscendentPrimalReward,
                        @BossLevelToTranscendentPrimalCap);";
                command.Parameters = new Dictionary<string, object>
                {
                    { "@UploadId", uploadId },
                    { "@TitanDamage", miscellaneousStatsModel.TitanDamage },
                    { "@SoulsSpent", miscellaneousStatsModel.HeroSoulsSpent },
                    { "@HeroSoulsSacrificed", miscellaneousStatsModel.HeroSoulsSacrificed },
                    { "@TotalAncientSouls", miscellaneousStatsModel.TotalAncientSouls },
                    { "@TranscendentPower", miscellaneousStatsModel.TranscendentPower },
                    { "@Rubies", miscellaneousStatsModel.Rubies },
                    { "@HighestZoneThisTranscension", miscellaneousStatsModel.HighestZoneThisTranscension },
                    { "@HighestZoneLifetime", miscellaneousStatsModel.HighestZoneLifetime },
                    { "@AscensionsThisTranscension", miscellaneousStatsModel.AscensionsThisTranscension },
                    { "@AscensionsLifetime", miscellaneousStatsModel.AscensionsLifetime },
                    { "@MaxTranscendentPrimalReward", miscellaneousStatsModel.MaxTranscendentPrimalReward },
                    { "@BossLevelToTranscendentPrimalCap", miscellaneousStatsModel.BossLevelToTranscendentPrimalCap },
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
                        { "@AncientId", pair.Key },
                        { "@Level", pair.Value.AncientLevel },
                    };
                    command.ExecuteNonQuery();
                }

                // Insert outsider levels
                foreach (var pair in outsiderLevels.OutsiderLevels)
                {
                    command.CommandText = @"
                        INSERT INTO OutsiderLevels(UploadId, OutsiderId, Level)
                        VALUES(@UploadId, @OutsiderId, @Level);";
                    command.Parameters = new Dictionary<string, object>
                    {
                        { "@UploadId", uploadId },
                        { "@OutsiderId", pair.Key },
                        { "@Level", pair.Value.Level },
                    };
                    command.ExecuteNonQuery();
                }

                var commited = command.CommitTransaction();
                if (!commited)
                {
                    return this.StatusCode((int)HttpStatusCode.InternalServerError);
                }
            }

            return this.Ok(uploadId);
        }

        /// <summary>
        /// Delete an upload.
        /// </summary>
        /// <returns>An appropriate status code with an empty response</returns>
        [Route("{uploadId:int}")]
        [HttpDelete]
        [Authorize]
        public IActionResult Delete(int uploadId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@UploadId", uploadId },
            };

            // First make sure the upload exists and belongs to the user
            const string GetUploadUserCommandText = @"
	            SELECT UserId
                FROM Uploads
                WHERE Id = @UploadId";
            using (var command = this.databaseCommandFactory.Create(
                GetUploadUserCommandText,
                parameters))
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    var uploadUserId = reader["UserId"].ToString();

                    var userId = this.userManager.GetUserId(this.User);
                    var isAdmin = this.User.IsInRole("Admin");

                    if (uploadUserId != userId && !isAdmin)
                    {
                        // Not this user's, so not allowed
                        return this.Unauthorized();
                    }
                }
                else
                {
                    // If we didn't get data, it's an upload that doesn't exist
                    return this.NotFound();
                }
            }

            // Perform the deletion from all tables
            const string DeleteUploadCommandText = @"
                DELETE
                FROM AncientLevels
                WHERE UploadId = @UploadId;

                DELETE
                FROM OutsiderLevels
                WHERE UploadId = @UploadId;

                DELETE
                FROM ComputedStats
                WHERE UploadId = @UploadId;

                DELETE
                FROM Uploads
                WHERE Id = @UploadId;";
            using (var command = this.databaseCommandFactory.Create(
                DeleteUploadCommandText,
                parameters))
            {
                command.ExecuteNonQuery();
            }

            return this.Ok();
        }

        private List<Upload> FetchUploads(string userId, int page, int count)
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
                var uploads = new List<Upload>(count);
                while (reader.Read())
                {
                    uploads.Add(new Upload
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