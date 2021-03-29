// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Data;
using System.Numerics;
using System.Text;
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

namespace ClickerHeroesTrackerWebsite.Controllers
{
    [Route("api/uploads")]
    [Authorize]
    [ApiController]
    public sealed class UploadsController : Controller
    {
        private readonly IDatabaseCommandFactory _databaseCommandFactory;

        private readonly GameData _gameData;

        private readonly IUserSettingsProvider _userSettingsProvider;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly IClanManager _clanManager;

        private readonly TelemetryClient _telemetryClient;

        public UploadsController(
            IDatabaseCommandFactory databaseCommandFactory,
            GameData gameData,
            IUserSettingsProvider userSettingsProvider,
            UserManager<ApplicationUser> userManager,
            IClanManager clanManager,
            TelemetryClient telemetryClient)
        {
            _databaseCommandFactory = databaseCommandFactory;
            _gameData = gameData;
            _userSettingsProvider = userSettingsProvider;
            _userManager = userManager;
            _clanManager = clanManager;
            _telemetryClient = telemetryClient;
        }

        [Route("{uploadId:int}")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<Upload>> DetailsAsync(int uploadId)
        {
            if (uploadId < 0)
            {
                return BadRequest();
            }

            Dictionary<string, object> uploadIdParameters = new()
            {
                { "@UploadId", uploadId },
            };

            string uploadUserId;
            string uploadContent;
            Upload upload = new() { Id = uploadId };
            const string GetUploadDataCommandText = @"
	            SELECT UserId, UserName, UploadTime, UploadContent, PlayStyle
                FROM Uploads
                LEFT JOIN AspNetUsers
                ON Uploads.UserId = AspNetUsers.Id
                WHERE Uploads.Id = @UploadId";
            using (IDatabaseCommand command = _databaseCommandFactory.Create(
                GetUploadDataCommandText,
                uploadIdParameters))
            using (IDataReader reader = await command.ExecuteReaderAsync())
            {
                if (reader.Read())
                {
                    uploadUserId = reader["UserId"].ToString();

                    string uploadUserName = reader["UserName"].ToString();
                    if (!string.IsNullOrEmpty(uploadUserName))
                    {
                        upload.User = new User
                        {
                            Name = uploadUserName,
                            ClanName = await _clanManager.GetClanNameAsync(uploadUserId),
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
                    return NotFound();
                }
            }

            bool isAdmin = User.IsInRole("Admin");
            bool isUploadAnonymous = upload.User == null;
            bool isOwn = !isUploadAnonymous && string.Equals(_userManager.GetUserId(User), uploadUserId, StringComparison.OrdinalIgnoreCase);

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
        public async Task<ActionResult<int>> AddAsync([FromForm] UploadRequest uploadRequest)
        {
            // Only associate it with the user if they requested that it be added to their progress.
            string userId = uploadRequest.AddToProgress && User.Identity.IsAuthenticated
                ? _userManager.GetUserId(User)
                : null;

            SavedGame savedGame = SavedGame.Parse(uploadRequest.EncodedSaveData);
            if (savedGame == null)
            {
                // Not a valid save
                return BadRequest();
            }

            // Kick off a clan update in parallel
            string gameUserId = savedGame.Object.Value<string>("uniqueId");
            string passwordHash = savedGame.Object.Value<string>("passwordHash");
            Task updateClanTask = _clanManager.UpdateClanAsync(userId, gameUserId, passwordHash);

            PlayStyle playStyle;
            if (uploadRequest.PlayStyle.HasValue)
            {
                playStyle = uploadRequest.PlayStyle.Value;
            }
            else
            {
                Website.Models.Api.Users.UserSettings userSettings = await _userSettingsProvider.GetAsync(userId);
                playStyle = userSettings.PlayStyle.GetValueOrDefault(PlayStyle.Hybrid);
            }

            // unixTimestamp is in milliseconds instead of seconds
            DateTime saveTime = (savedGame.Object.Value<double>("unixTimestamp") / 1000).UnixTimeStampToDateTime();

            AncientLevelsModel ancientLevels = new(
                _gameData,
                savedGame);
            OutsiderLevelsModel outsiderLevels = new(
                _gameData,
                savedGame);
            ComputedStats computedStats = new(savedGame);

            int uploadId;
            using (IDatabaseCommand command = _databaseCommandFactory.Create())
            {
                await command.BeginTransactionAsync();

                // Insert Upload
                command.CommandText = @"
	                INSERT INTO Uploads(UserId, UploadContent, PlayStyle, SaveTime)
                    VALUES(@UserId, @UploadContent, @PlayStyle, @SaveTime);
                    SELECT SCOPE_IDENTITY();";
                command.Parameters = new Dictionary<string, object>
                {
                    { "@UserId", userId },
                    { "@UploadContent", uploadRequest.EncodedSaveData },
                    { "@PlayStyle", playStyle.ToString() },
                    { "@SaveTime", saveTime },
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
                StringBuilder ancientLevelsCommandText = new("INSERT INTO AncientLevels(UploadId, AncientId, Level) VALUES");
                Dictionary<string, object> ancientLevelsParameters = new()
                {
                    { "@UploadId", uploadId },
                };
                bool isFirstAncient = true;
                foreach (KeyValuePair<int, BigInteger> pair in ancientLevels.AncientLevels)
                {
                    if (!isFirstAncient)
                    {
                        ancientLevelsCommandText.Append(',');
                    }

                    // Looks like: (@UploadId, @AncientId{AncientId}, @AncientLevel{AncientId})
                    string idParamName = "@AncientId" + pair.Key;
                    string levelParamName = "@AncientLevel" + pair.Key;
                    ancientLevelsCommandText.Append("(@UploadId,");
                    ancientLevelsCommandText.Append(idParamName);
                    ancientLevelsCommandText.Append(',');
                    ancientLevelsCommandText.Append(levelParamName);
                    ancientLevelsCommandText.Append(')');

                    ancientLevelsParameters.Add(idParamName, pair.Key);
                    ancientLevelsParameters.Add(levelParamName, pair.Value.ToTransportableString());

                    isFirstAncient = false;
                }

                command.CommandText = ancientLevelsCommandText.ToString();
                command.Parameters = ancientLevelsParameters;
                await command.ExecuteNonQueryAsync();

                // Insert outsider levels
                StringBuilder outsiderLevelsCommandText = new("INSERT INTO OutsiderLevels(UploadId, OutsiderId, Level) VALUES");
                Dictionary<string, object> outsiderLevelsParameters = new()
                {
                    { "@UploadId", uploadId },
                };
                bool isFirstOutsider = true;
                foreach (KeyValuePair<int, long> pair in outsiderLevels.OutsiderLevels)
                {
                    if (!isFirstOutsider)
                    {
                        outsiderLevelsCommandText.Append(',');
                    }

                    // Looks like: (@UploadId, @OutsiderId{OutsiderId}, @Level{OutsiderId})
                    string idParamName = "@OutsiderId" + pair.Key;
                    string levelParamName = "@OutsiderLevel" + pair.Key;
                    outsiderLevelsCommandText.Append("(@UploadId,");
                    outsiderLevelsCommandText.Append(idParamName);
                    outsiderLevelsCommandText.Append(',');
                    outsiderLevelsCommandText.Append(levelParamName);
                    outsiderLevelsCommandText.Append(')');

                    outsiderLevelsParameters.Add(idParamName, pair.Key);
                    outsiderLevelsParameters.Add(levelParamName, pair.Value);

                    isFirstOutsider = false;
                }

                command.CommandText = outsiderLevelsCommandText.ToString();
                command.Parameters = outsiderLevelsParameters;
                await command.ExecuteNonQueryAsync();

                command.CommitTransaction();
            }

            // Wait for the task to finish, but don't fail the request if it fails
            try
            {
                await updateClanTask;
            }
            catch (Exception e)
            {
                Dictionary<string, string> properties = new()
                {
                    { "UploadId", uploadId.ToString() },
                };
                _telemetryClient.TrackException(e, properties);
            }

            return uploadId;
        }

        [Route("{uploadId:int}")]
        [HttpDelete]
        public async Task<ActionResult> DeleteAsync(int uploadId)
        {
            Dictionary<string, object> parameters = new()
            {
                { "@UploadId", uploadId },
            };

            // First make sure the upload exists and belongs to the user
            const string GetUploadUserCommandText = @"
	            SELECT UserId
                FROM Uploads
                WHERE Id = @UploadId";
            using (IDatabaseCommand command = _databaseCommandFactory.Create(
                GetUploadUserCommandText,
                parameters))
            using (IDataReader reader = await command.ExecuteReaderAsync())
            {
                if (reader.Read())
                {
                    string uploadUserId = reader["UserId"].ToString();

                    string userId = _userManager.GetUserId(User);
                    bool isAdmin = User.IsInRole("Admin");

                    if (!string.Equals(uploadUserId, userId, StringComparison.OrdinalIgnoreCase) && !isAdmin)
                    {
                        // Not this user's, so not allowed
                        return Forbid();
                    }
                }
                else
                {
                    // If we didn't get data, it's an upload that doesn't exist
                    return NotFound();
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
            using (IDatabaseCommand command = _databaseCommandFactory.Create(
                DeleteUploadCommandText,
                parameters))
            {
                await command.ExecuteNonQueryAsync();
            }

            return Ok();
        }
    }
}