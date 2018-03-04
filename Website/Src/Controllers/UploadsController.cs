// <copyright file="UploadsController.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ClickerHeroesTrackerWebsite.Models;
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
    using Website.Services.Clans;

    [Route("api/uploads")]
    [Authorize]
    [ApiController]
    public sealed class UploadsController : Controller
    {
        private readonly IDatabaseCommandFactory databaseCommandFactory;

        private readonly GameData gameData;

        private readonly IUserSettingsProvider userSettingsProvider;

        private readonly UserManager<ApplicationUser> userManager;

        private readonly IClanManager clanManager;

        private readonly TelemetryClient telemetryClient;

        public UploadsController(
            IDatabaseCommandFactory databaseCommandFactory,
            GameData gameData,
            IUserSettingsProvider userSettingsProvider,
            UserManager<ApplicationUser> userManager,
            IClanManager clanManager,
            TelemetryClient telemetryClient)
        {
            this.databaseCommandFactory = databaseCommandFactory;
            this.gameData = gameData;
            this.userSettingsProvider = userSettingsProvider;
            this.userManager = userManager;
            this.clanManager = clanManager;
            this.telemetryClient = telemetryClient;
        }

        [Route("{uploadId:int}")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<Upload>> Details(int uploadId)
        {
            if (uploadId < 0)
            {
                return this.BadRequest();
            }

            var uploadIdParameters = new Dictionary<string, object>
            {
                { "@UploadId", uploadId },
            };

            string uploadUserId;
            string uploadContent;
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
            using (var reader = await command.ExecuteReaderAsync())
            {
                if (reader.Read())
                {
                    uploadUserId = reader["UserId"].ToString();

                    var uploadUserName = reader["UserName"].ToString();
                    if (!string.IsNullOrEmpty(uploadUserName))
                    {
                        upload.User = new User
                        {
                            Name = uploadUserName,
                            ClanName = await this.clanManager.GetClanNameAsync(uploadUserId),
                        };
                    }

                    // The DateTime is a datetime2 which has no timezone so comes out as DateTimeKind.Unknown. Se need to specify the kind so it gets serialized correctly.
                    upload.TimeSubmitted = DateTime.SpecifyKind(Convert.ToDateTime(reader["UploadTime"]), DateTimeKind.Utc);

                    uploadContent = reader["UploadContent"].ToString();
                    upload.PlayStyle = reader["PlayStyle"].ToString().SafeParseEnum<PlayStyle>();
                }
                else
                {
                    // If we didn't get data, it's an upload that doesn't exist
                    return this.NotFound();
                }
            }

            var isAdmin = this.User.IsInRole("Admin");
            var isUploadAnonymous = upload.User == null;
            var isOwn = !isUploadAnonymous && string.Equals(this.userManager.GetUserId(this.User), uploadUserId, StringComparison.OrdinalIgnoreCase);
            var uploadUserSettings = await this.userSettingsProvider.GetAsync(uploadUserId);
            var isPublic = isUploadAnonymous || uploadUserSettings.AreUploadsPublic.GetValueOrDefault(true);
            var isPermitted = isOwn || isPublic || isAdmin;

            if (!isPermitted)
            {
                return this.Forbid();
            }

            // Only return the raw upload content if it's the requesting user's or an admin requested it.
            if (isOwn || isAdmin)
            {
                upload.Content = uploadContent;
            }
            else
            {
                upload.Content = SavedGame.ScrubIdentity(uploadContent);
                upload.IsScrubbed = true;
            }

            return upload;
        }

        [Route("")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<int>> Add([FromForm] UploadRequest uploadRequest)
        {
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

            // Kick off a clan update in parallel
            var gameUserId = savedGame.Object.Value<string>("uniqueId");
            var passwordHash = savedGame.Object.Value<string>("passwordHash");
            var updateClanTask = this.clanManager.UpdateClanAsync(userId, gameUserId, passwordHash);

            PlayStyle playStyle;
            if (uploadRequest.PlayStyle.HasValue)
            {
                playStyle = uploadRequest.PlayStyle.Value;
            }
            else
            {
                var userSettings = await this.userSettingsProvider.GetAsync(userId);
                playStyle = userSettings.PlayStyle.GetValueOrDefault(PlayStyle.Hybrid);
            }

            var ancientLevels = new AncientLevelsModel(
                this.gameData,
                savedGame);
            var outsiderLevels = new OutsiderLevelsModel(
                this.gameData,
                savedGame);
            var computedStats = new ComputedStats(savedGame);

            int uploadId;
            using (var command = this.databaseCommandFactory.Create())
            {
                await command.BeginTransactionAsync();

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
                uploadId = Convert.ToInt32(await command.ExecuteScalarAsync());

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
                        AscensionsLifetime)
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
                        @AscensionsLifetime);";
                command.Parameters = new Dictionary<string, object>
                {
                    { "@UploadId", uploadId },
                    { "@TitanDamage", computedStats.TitanDamage },
                    { "@SoulsSpent", computedStats.HeroSoulsSpent },
                    { "@HeroSoulsSacrificed", computedStats.HeroSoulsSacrificed },
                    { "@TotalAncientSouls", computedStats.TotalAncientSouls },
                    { "@TranscendentPower", computedStats.TranscendentPower },
                    { "@Rubies", computedStats.Rubies },
                    { "@HighestZoneThisTranscension", computedStats.HighestZoneThisTranscension },
                    { "@HighestZoneLifetime", computedStats.HighestZoneLifetime },
                    { "@AscensionsThisTranscension", computedStats.AscensionsThisTranscension },
                    { "@AscensionsLifetime", computedStats.AscensionsLifetime },
                };
                await command.ExecuteNonQueryAsync();

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
                        { "@Level", pair.Value.ToTransportableString() },
                    };
                    await command.ExecuteNonQueryAsync();
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
                        { "@Level", pair.Value },
                    };
                    await command.ExecuteNonQueryAsync();
                }

                command.CommitTransaction();
            }

            // Wait for the task to finish, but don't fail the request if it fails
            try
            {
                await updateClanTask;
            }
            catch (Exception e)
            {
                var properties = new Dictionary<string, string>
                {
                    { "UploadId", uploadId.ToString() },
                };
                this.telemetryClient.TrackException(e, properties);
            }

            return uploadId;
        }

        [Route("{uploadId:int}")]
        [HttpDelete]
        public async Task<ActionResult> Delete(int uploadId)
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
            using (var reader = await command.ExecuteReaderAsync())
            {
                if (reader.Read())
                {
                    var uploadUserId = reader["UserId"].ToString();

                    var userId = this.userManager.GetUserId(this.User);
                    var isAdmin = this.User.IsInRole("Admin");

                    if (!string.Equals(uploadUserId, userId, StringComparison.OrdinalIgnoreCase) && !isAdmin)
                    {
                        // Not this user's, so not allowed
                        return this.Forbid();
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
                await command.ExecuteNonQueryAsync();
            }

            return this.Ok();
        }
    }
}