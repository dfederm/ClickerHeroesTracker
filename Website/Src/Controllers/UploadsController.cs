// <copyright file="UploadsController.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using ClickerHeroesTrackerWebsite.Instrumentation;
    using ClickerHeroesTrackerWebsite.Models;
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

    [Route("api/uploads")]
    [Authorize]
    public sealed class UploadsController : Controller
    {
        private readonly IDatabaseCommandFactory databaseCommandFactory;

        private readonly GameData gameData;

        private readonly IUserSettingsProvider userSettingsProvider;

        private readonly TelemetryClient telemetryClient;

        private readonly ICounterProvider counterProvider;

        private readonly UserManager<ApplicationUser> userManager;

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

        [Route("{uploadId:int}")]
        [HttpGet]
        [AllowAnonymous]
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

                    // The DateTime is a datetime2 which has no timezone so comes out as DateTimeKind.Unknown. Se need to specify the kind so it gets serialized correctly.
                    upload.TimeSubmitted = DateTime.SpecifyKind(Convert.ToDateTime(reader["UploadTime"]), DateTimeKind.Utc);

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
            var isPublic = isUploadAnonymous || uploadUserSettings.AreUploadsPublic.GetValueOrDefault(true);
            var isPermitted = isOwn || isPublic || isAdmin;

            if (!isPermitted)
            {
                return this.Forbid();
            }

            // Only return the raw upload content if it's the requesting user's or an admin requested it.
            if (isOwn || isAdmin)
            {
                upload.UploadContent = uploadContent;
            }

            // Set the play style.
            upload.PlayStyle = playStyle;

            var savedGame = SavedGame.Parse(uploadContent);
            upload.Stats = new Dictionary<StatType, string>();

            // Get ancient level stats
            var ancientLevelsModel = new AncientLevelsModel(
                this.gameData,
                savedGame,
                this.telemetryClient);
            foreach (var ancientLevelInfo in ancientLevelsModel.AncientLevels)
            {
                var ancientLevel = ancientLevelInfo.Value.AncientLevel;
                if (ancientLevel > 0)
                {
                    upload.Stats.Add(AncientIds.GetAncientStatType(ancientLevelInfo.Key), ancientLevel.ToTransportableString());
                }

                var itemLevel = ancientLevelInfo.Value.ItemLevel;
                if (itemLevel > 0)
                {
                    upload.Stats.Add(AncientIds.GetItemStatType(ancientLevelInfo.Key), itemLevel.ToString());
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
                    upload.Stats.Add(OutsiderIds.GetOusiderStatType(pair.Key), outsiderLevel.ToString());
                }
            }

            // Get misc stats
            var miscellaneousStatsModel = new MiscellaneousStatsModel(savedGame);
            upload.Stats.Add(StatType.AscensionsLifetime, miscellaneousStatsModel.AscensionsLifetime.ToString());
            upload.Stats.Add(StatType.AscensionsThisTranscension, miscellaneousStatsModel.AscensionsThisTranscension.ToString());
            upload.Stats.Add(StatType.HeroSoulsSacrificed, miscellaneousStatsModel.HeroSoulsSacrificed.ToTransportableString());
            upload.Stats.Add(StatType.HeroSoulsSpent, miscellaneousStatsModel.HeroSoulsSpent.ToTransportableString());
            upload.Stats.Add(StatType.HighestZoneLifetime, miscellaneousStatsModel.HighestZoneLifetime.ToString());
            upload.Stats.Add(StatType.HighestZoneThisTranscension, miscellaneousStatsModel.HighestZoneThisTranscension.ToString());
            upload.Stats.Add(StatType.Rubies, miscellaneousStatsModel.Rubies.ToString());
            upload.Stats.Add(StatType.TitanDamage, miscellaneousStatsModel.TitanDamage.ToTransportableString());
            upload.Stats.Add(StatType.TotalAncientSouls, miscellaneousStatsModel.TotalAncientSouls.ToString());
            upload.Stats.Add(StatType.TranscendentPower, miscellaneousStatsModel.TranscendentPower.ToString());
            upload.Stats.Add(StatType.HeroSouls, miscellaneousStatsModel.HeroSouls.ToTransportableString());
            upload.Stats.Add(StatType.PendingSouls, miscellaneousStatsModel.PendingSouls.ToTransportableString());
            upload.Stats.Add(StatType.Autoclickers, miscellaneousStatsModel.Autoclickers.ToString());

            return this.Ok(upload);
        }

        [Route("")]
        [HttpPost]
        [AllowAnonymous]
        public IActionResult Add(UploadRequest uploadRequest)
        {
            if (uploadRequest.EncodedSaveData == null)
            {
                // Not a valid save
                return this.BadRequest();
            }

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
                playStyle = userSettings.PlayStyle.GetValueOrDefault(PlayStyle.Hybrid);
            }

            var ancientLevels = new AncientLevelsModel(
                this.gameData,
                savedGame,
                this.telemetryClient);
            var outsiderLevels = new OutsiderLevelsModel(
                this.gameData,
                savedGame,
                this.telemetryClient);
            var miscellaneousStatsModel = new MiscellaneousStatsModel(savedGame);

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
                    { "@TitanDamage", miscellaneousStatsModel.TitanDamage.ToTransportableString() },
                    { "@SoulsSpent", miscellaneousStatsModel.HeroSoulsSpent.ToTransportableString() },
                    { "@HeroSoulsSacrificed", miscellaneousStatsModel.HeroSoulsSacrificed.ToTransportableString() },
                    { "@TotalAncientSouls", miscellaneousStatsModel.TotalAncientSouls },
                    { "@TranscendentPower", miscellaneousStatsModel.TranscendentPower },
                    { "@Rubies", miscellaneousStatsModel.Rubies },
                    { "@HighestZoneThisTranscension", miscellaneousStatsModel.HighestZoneThisTranscension },
                    { "@HighestZoneLifetime", miscellaneousStatsModel.HighestZoneLifetime },
                    { "@AscensionsThisTranscension", miscellaneousStatsModel.AscensionsThisTranscension },
                    { "@AscensionsLifetime", miscellaneousStatsModel.AscensionsLifetime },
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
                        { "@Level", pair.Value.AncientLevel.ToTransportableString() },
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

        [Route("{uploadId:int}")]
        [HttpDelete]
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
                command.ExecuteNonQuery();
            }

            return this.Ok();
        }
    }
}