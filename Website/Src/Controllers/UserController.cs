// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClickerHeroesTrackerWebsite.Models;
using ClickerHeroesTrackerWebsite.Models.Api;
using ClickerHeroesTrackerWebsite.Models.Api.Uploads;
using ClickerHeroesTrackerWebsite.Models.Api.Users;
using ClickerHeroesTrackerWebsite.Models.Game;
using ClickerHeroesTrackerWebsite.Models.Settings;
using ClickerHeroesTrackerWebsite.Services.Database;
using ClickerHeroesTrackerWebsite.Services.Email;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Website.Models.Api.Users;
using Website.Services.Clans;

namespace Website.Controllers
{
    [Route("api/users")]
    [Authorize]
    [ApiController]
    public class UserController : Controller
    {
        private readonly GameData _gameData;

        private readonly TelemetryClient _telemetryClient;

        private readonly IDatabaseCommandFactory _databaseCommandFactory;

        private readonly IUserSettingsProvider _userSettingsProvider;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly IEmailSender _emailSender;

        private readonly IClanManager _clanManager;

        public UserController(
            GameData gameData,
            TelemetryClient telemetryClient,
            IDatabaseCommandFactory databaseCommandFactory,
            IUserSettingsProvider userSettingsProvider,
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender,
            IClanManager clanManager)
        {
            _gameData = gameData;
            _telemetryClient = telemetryClient;
            _databaseCommandFactory = databaseCommandFactory;
            _userSettingsProvider = userSettingsProvider;
            _userManager = userManager;
            _emailSender = emailSender;
            _clanManager = clanManager;
        }

        [Route("")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> CreateAsync(CreateUserRequest createUser)
        {
            ApplicationUser user = new() { UserName = createUser.UserName, Email = createUser.Email };
            IdentityResult result = await _userManager.CreateAsync(user, createUser.Password);
            if (result.Succeeded)
            {
                return Ok();
            }

            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return BadRequest(ModelState);
        }

        [Route("{userName}")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<User>> GetAsync(string userName)
        {
            ApplicationUser user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return NotFound();
            }

            string userId = await _userManager.GetUserIdAsync(user);
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound();
            }

            return new User
            {
                Name = user.UserName,
                ClanName = await _clanManager.GetClanNameAsync(userId),
            };
        }

        [Route("{userName}/uploads")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<UploadSummaryListResponse>> UploadsAsync(
            string userName,
            int page = ParameterConstants.Uploads.Page.Default,
            int count = ParameterConstants.Uploads.Count.Default)
        {
            // Validate parameters
            if (page < ParameterConstants.Uploads.Page.Min)
            {
                return BadRequest();
            }

            if (count < ParameterConstants.Uploads.Count.Min
                || count > ParameterConstants.Uploads.Count.Max)
            {
                return BadRequest();
            }

            ApplicationUser user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return NotFound();
            }

            string userId = await _userManager.GetUserIdAsync(user);
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound();
            }

            const string GetUploadsCommandText = @"
                SELECT Id,
                    UploadTime,
                    SaveTime,
                    AscensionsLifetime AS AscensionNumber,
                    HighestZoneThisTranscension AS Zone,
                    RTRIM(SoulsSpent) AS Souls
                FROM Uploads
                INNER JOIN ComputedStats
                ON ComputedStats.UploadId = Uploads.Id
                WHERE UserId = @UserId
                ORDER BY SaveTime DESC
                OFFSET @Offset ROWS
                FETCH NEXT @Count ROWS ONLY;";
            Dictionary<string, object> getUploadsCommandparameters = new()
            {
                { "@UserId", userId },
                { "@Offset", (page - 1) * count },
                { "@Count", count },
            };

            List<UploadSummary> uploads = new(count);
            using (IDatabaseCommand command = _databaseCommandFactory.Create(GetUploadsCommandText, getUploadsCommandparameters))
            using (IDataReader reader = await command.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    uploads.Add(new UploadSummary
                    {
                        Id = Convert.ToInt32(reader["Id"]),

                        // The DateTime is a datetime2 which has no timezone so comes out as DateTimeKind.Unknown. Se need to specify the kind so it gets serialized correctly.
                        TimeSubmitted = DateTime.SpecifyKind(Convert.ToDateTime(reader["UploadTime"]), DateTimeKind.Utc),
                        SaveTime = DateTime.SpecifyKind(Convert.ToDateTime(reader["SaveTime"]), DateTimeKind.Utc),

                        AscensionNumber = Convert.ToInt32(reader["AscensionNumber"]),
                        Zone = Convert.ToInt32(reader["Zone"]),
                        Souls = reader["Souls"].ToString(),
                    });
                }
            }

            const string GetUploadCountCommandText = @"
                SELECT COUNT(*) AS TotalUploads
                FROM Uploads
                WHERE UserId = @UserId";
            Dictionary<string, object> getUploadCountparameters = new()
            {
                { "@UserId", userId },
            };

            PaginationMetadata pagination = new();
            using (IDatabaseCommand command = _databaseCommandFactory.Create(GetUploadCountCommandText, getUploadCountparameters))
            using (IDataReader reader = await command.ExecuteReaderAsync())
            {
                if (reader.Read())
                {
                    pagination.Count = Convert.ToInt32(reader["TotalUploads"]);

                    Microsoft.AspNetCore.Http.PathString currentPath = Request.Path;
                    if (page > 1)
                    {
                        pagination.Previous = $"{currentPath}?{nameof(page)}={page - 1}&{nameof(count)}={count}";
                    }

                    if (page <= Math.Ceiling((float)pagination.Count / count))
                    {
                        pagination.Next = $"{currentPath}?{nameof(page)}={page + 1}&{nameof(count)}={count}";
                    }
                }
            }

            return new UploadSummaryListResponse
            {
                Uploads = uploads,
                Pagination = pagination,
            };
        }

        [Route("{userName}/progress")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ProgressData>> ProgressAsync(
            string userName,
            DateTime? start,
            DateTime? end,
            int? page,
            int? count)
        {
            if ((start.HasValue || end.HasValue) && (page.HasValue || count.HasValue))
            {
                ModelState.AddModelError(string.Empty, "time-based and ascension-based filters are mutually exclusive");
                return BadRequest(ModelState);
            }

            ApplicationUser user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return NotFound();
            }

            string userId = await _userManager.GetUserIdAsync(user);
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound();
            }

            Dictionary<string, object> parameters = new()
            {
                { "@UserId", userId },
            };

            StringBuilder commandText = new();
            Func<object, string> parseOrdinal;

            // If either count or skip are set, use ascention-based filtering. Default to time-based in all other cases.
            // Note that we're inferring the type from the request and don't really care what the user's settings are. The client
            // should take settings into account and make the appropriate API call.
            if (page.HasValue || count.HasValue)
            {
                // Fill in missing range values as needed
                int pageNum = page.GetValueOrDefault(ParameterConstants.Progress.Page.Default);
                int countNum = count.GetValueOrDefault(ParameterConstants.Progress.Count.Default);

                // Validate parameters
                if (pageNum < ParameterConstants.Progress.Page.Min)
                {
                    return BadRequest();
                }

                if (countNum < ParameterConstants.Progress.Count.Min
                    || countNum > ParameterConstants.Progress.Count.Max)
                {
                    return BadRequest();
                }

                parameters.Add("@Offset", (pageNum - 1) * countNum);
                parameters.Add("@Count", countNum);

                commandText.Append(@"
                    -- Create a temp table that scopes the Uploads
                    CREATE TABLE #ScopedUploads
                    (
                        Id  INT NOT NULL,
                        Ordinal FLOAT (53) NOT NULL,
                    );

                    -- Populate temp table
                    INSERT INTO #ScopedUploads (Id, Ordinal)
                    SELECT Id, AscensionsLifetime AS Ordinal
                    FROM Uploads
                    INNER JOIN ComputedStats
                    ON ComputedStats.UploadId = Uploads.Id
                    WHERE UserId = @UserId
                    ORDER BY Ordinal DESC
                    OFFSET @Offset ROWS
                    FETCH NEXT @Count ROWS ONLY;");

                parseOrdinal = rawOrdinal => rawOrdinal.ToString();
            }
            else
            {
                // Fill in missing range values as needed
                DateTime startTime;
                DateTime endTime;
                if (start.HasValue && end.HasValue)
                {
                    startTime = start.Value;
                    endTime = end.Value;
                }
                else if (start.HasValue)
                {
                    // Default to a week after the start date
                    startTime = start.Value;
                    endTime = start.Value.AddDays(7);
                }
                else if (end.HasValue)
                {
                    // Default to a week before the end date
                    startTime = end.Value.AddDays(-7);
                    endTime = end.Value;
                }
                else
                {
                    // Default to the past week
                    DateTime now = DateTime.UtcNow;
                    startTime = now.AddDays(-7);
                    endTime = now;
                }

                // SQL's datetime2 has no timezone so we need to explicitly convert to UTC
                startTime = startTime.ToUniversalTime();
                endTime = endTime.ToUniversalTime();

                parameters.Add("@StartTime", startTime);
                parameters.Add("@EndTime", endTime);

                commandText.Append(@"
                    -- Create a temp table that scopes the Uploads
                    CREATE TABLE #ScopedUploads
                    (
                        Id  INT NOT NULL,
                        Ordinal DATETIME2(0) NOT NULL,
                    );

                    -- Populate temp table
                    INSERT INTO #ScopedUploads (Id, Ordinal)
                    SELECT Id, SaveTime AS Ordinal
                    FROM Uploads
                    WHERE UserId = @UserId
                    AND SaveTime >= ISNULL(@StartTime, '0001-01-01 00:00:00')
                    AND SaveTime <= ISNULL(@EndTime, '9999-12-31 23:59:59');");

                // The DateTime is a datetime2 which has no timezone so comes out as DateTimeKind.Unknown. Se need to specify the kind so it gets serialized correctly.
                // For the ToString, use a modified ISO 8601 which excludes the milliseconds.
                parseOrdinal = rawOrdinal => DateTime.SpecifyKind(Convert.ToDateTime(rawOrdinal), DateTimeKind.Utc).ToString("yyyy-MM-ddTHH:mm:ssK");
            }

            commandText.Append(@"
                -- Computed Stats
                SELECT #ScopedUploads.Ordinal,
                       RTRIM(ComputedStats.TitanDamage) AS TitanDamage,
                       RTRIM(ComputedStats.SoulsSpent) AS SoulsSpent,
                       RTRIM(ComputedStats.HeroSoulsSacrificed) AS HeroSoulsSacrificed,
                       ComputedStats.TotalAncientSouls,
                       ComputedStats.TranscendentPower,
                       ComputedStats.Rubies,
                       ComputedStats.HighestZoneThisTranscension,
                       ComputedStats.HighestZoneLifetime,
                       ComputedStats.AscensionsThisTranscension,
                       ComputedStats.AscensionsLifetime
                FROM ComputedStats
                INNER JOIN #ScopedUploads
                ON ComputedStats.UploadId = #ScopedUploads.Id;

                -- Ancient Levels
                SELECT #ScopedUploads.Ordinal, AncientLevels.AncientId, RTRIM(AncientLevels.Level) AS Level
                FROM AncientLevels
                INNER JOIN #ScopedUploads
                ON AncientLevels.UploadId = #ScopedUploads.Id;

                -- Outsider Levels
                SELECT #ScopedUploads.Ordinal, OutsiderLevels.OutsiderId, OutsiderLevels.Level
                FROM OutsiderLevels
                INNER JOIN #ScopedUploads
                ON OutsiderLevels.UploadId = #ScopedUploads.Id;

                -- Drop the temp table
                DROP TABLE #ScopedUploads;");

            using (IDatabaseCommand command = _databaseCommandFactory.Create(commandText.ToString(), parameters))
            using (IDataReader reader = await command.ExecuteReaderAsync())
            {
                ProgressData progressData = new()
                {
                    TitanDamageData = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                    SoulsSpentData = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                    HeroSoulsSacrificedData = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                    TotalAncientSoulsData = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                    TranscendentPowerData = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                    RubiesData = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                    HighestZoneThisTranscensionData = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                    HighestZoneLifetimeData = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                    AscensionsThisTranscensionData = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                    AscensionsLifetimeData = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                    AncientLevelData = new SortedDictionary<string, IDictionary<string, string>>(StringComparer.OrdinalIgnoreCase),
                    OutsiderLevelData = new SortedDictionary<string, IDictionary<string, string>>(StringComparer.OrdinalIgnoreCase),
                };

                while (reader.Read())
                {
                    string ordinal = parseOrdinal(reader["Ordinal"]);

                    progressData.TitanDamageData[ordinal] = reader["TitanDamage"].ToString();
                    progressData.SoulsSpentData[ordinal] = reader["SoulsSpent"].ToString();
                    progressData.HeroSoulsSacrificedData[ordinal] = reader["HeroSoulsSacrificed"].ToString();
                    progressData.TotalAncientSoulsData[ordinal] = reader["TotalAncientSouls"].ToString();
                    progressData.TranscendentPowerData[ordinal] = (100 * Convert.ToDouble(reader["TranscendentPower"])).ToString();
                    progressData.RubiesData[ordinal] = reader["Rubies"].ToString();
                    progressData.HighestZoneThisTranscensionData[ordinal] = reader["HighestZoneThisTranscension"].ToString();
                    progressData.HighestZoneLifetimeData[ordinal] = reader["HighestZoneLifetime"].ToString();
                    progressData.AscensionsThisTranscensionData[ordinal] = reader["AscensionsThisTranscension"].ToString();
                    progressData.AscensionsLifetimeData[ordinal] = reader["AscensionsLifetime"].ToString();
                }

                if (!reader.NextResult())
                {
                    return StatusCode(500);
                }

                while (reader.Read())
                {
                    string ordinal = parseOrdinal(reader["Ordinal"]);
                    int ancientId = Convert.ToInt32(reader["AncientId"]);
                    string level = reader["Level"].ToString();

                    if (!_gameData.Ancients.TryGetValue(ancientId, out Ancient ancient))
                    {
                        _telemetryClient.TrackEvent("Unknown Ancient", new Dictionary<string, string> { { "AncientId", ancientId.ToString() } });
                        continue;
                    }

                    if (!progressData.AncientLevelData.TryGetValue(ancient.Name, out IDictionary<string, string> levelData))
                    {
                        levelData = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        progressData.AncientLevelData.Add(ancient.Name, levelData);
                    }

                    levelData[ordinal] = level;
                }

                if (!reader.NextResult())
                {
                    return StatusCode(500);
                }

                while (reader.Read())
                {
                    string ordinal = parseOrdinal(reader["Ordinal"]);
                    int outsiderId = Convert.ToInt32(reader["OutsiderId"]);
                    string level = reader["Level"].ToString();

                    if (!_gameData.Outsiders.TryGetValue(outsiderId, out Outsider outsider))
                    {
                        _telemetryClient.TrackEvent("Unknown Outsider", new Dictionary<string, string> { { "OutsiderId", outsiderId.ToString() } });
                        continue;
                    }

                    if (!progressData.OutsiderLevelData.TryGetValue(outsider.Name, out IDictionary<string, string> levelData))
                    {
                        levelData = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        progressData.OutsiderLevelData.Add(outsider.Name, levelData);
                    }

                    levelData[ordinal] = level;
                }

                return progressData;
            }
        }

        [Route("{userName}/follows")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<FollowsData>> FollowsAsync(string userName)
        {
            ApplicationUser user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return NotFound();
            }

            string userId = await _userManager.GetUserIdAsync(user);
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound();
            }

            List<string> follows = new();
            Dictionary<string, object> parameters = new()
            {
                { "@UserId", userId },
            };
            const string GetUserFollowsCommandText = @"
                SELECT UserName
                FROM UserFollows
                INNER JOIN AspNetUsers
                ON UserFollows.FollowUserId = AspNetUsers.Id
                WHERE UserId = @UserId
                ORDER BY UserName ASC";
            using (IDatabaseCommand command = _databaseCommandFactory.Create(
                GetUserFollowsCommandText,
                parameters))
            using (IDataReader reader = await command.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    follows.Add(reader["UserName"].ToString());
                }
            }

            return new FollowsData
            {
                Follows = follows,
            };
        }

        [Route("{userName}/follows")]
        [HttpPost]
        public async Task<ActionResult> AddFollowAsync(string userName, AddFollowRequest model)
        {
            ApplicationUser user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return NotFound();
            }

            string userId = await _userManager.GetUserIdAsync(user);
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound();
            }

            string currentUserId = _userManager.GetUserId(User);
            if (!userId.Equals(currentUserId, StringComparison.OrdinalIgnoreCase)
                && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            ApplicationUser followUser = await _userManager.FindByNameAsync(model.FollowUserName);
            if (followUser == null)
            {
                return NotFound();
            }

            string followUserId = await _userManager.GetUserIdAsync(followUser);
            if (string.IsNullOrEmpty(followUserId))
            {
                return NotFound();
            }

            Dictionary<string, object> parameters = new()
            {
                { "@UserId", userId },
                { "@FollowUserId", followUserId },
            };
            const string CommandText = @"
                MERGE INTO UserFollows WITH (HOLDLOCK)
                USING
                    (VALUES (@UserId, @FollowUserId))
                        AS Input(UserId, FollowUserId)
                    ON UserFollows.UserId = Input.UserId
                    AND UserFollows.FollowUserId = Input.FollowUserId
                WHEN NOT MATCHED THEN
                    INSERT (UserId, FollowUserId)
                    VALUES(@UserId, @FollowUserId);";
            using (IDatabaseCommand command = _databaseCommandFactory.Create(
                CommandText,
                parameters))
            {
                await command.ExecuteNonQueryAsync();
            }

            return Ok();
        }

        [Route("{userName}/follows/{followUserName}")]
        [HttpDelete]
        public async Task<ActionResult> RemoveFollowAsync(string userName, string followUserName)
        {
            ApplicationUser user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return NotFound();
            }

            string userId = await _userManager.GetUserIdAsync(user);
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound();
            }

            string currentUserId = _userManager.GetUserId(User);
            if (!userId.Equals(currentUserId, StringComparison.OrdinalIgnoreCase)
                && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            ApplicationUser followUser = await _userManager.FindByNameAsync(followUserName);
            if (followUser == null)
            {
                return NotFound();
            }

            string followUserId = await _userManager.GetUserIdAsync(followUser);
            if (string.IsNullOrEmpty(followUserId))
            {
                return NotFound();
            }

            Dictionary<string, object> parameters = new()
            {
                { "@UserId", userId },
                { "@FollowUserId", followUserId },
            };
            const string CommandText = @"
                DELETE
                FROM UserFollows
                WHERE UserId = @UserId
                AND FollowUserId = @FollowUserId;
                SELECT @@ROWCOUNT;";
            using (IDatabaseCommand command = _databaseCommandFactory.Create(
                CommandText,
                parameters))
            {
                int numDeletions = Convert.ToInt32(await command.ExecuteScalarAsync());
                if (numDeletions == 0)
                {
                    return NotFound();
                }
            }

            return Ok();
        }

        [Route("{userName}/settings")]
        [HttpGet]
        public async Task<ActionResult<UserSettings>> GetSettingsAsync(string userName)
        {
            ApplicationUser user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return NotFound();
            }

            string userId = await _userManager.GetUserIdAsync(user);
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound();
            }

            string currentUserId = _userManager.GetUserId(User);
            if (!userId.Equals(currentUserId, StringComparison.OrdinalIgnoreCase)
                && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return await _userSettingsProvider.GetAsync(userId);
        }

        [Route("{userName}/settings")]
        [HttpPatch]
        public async Task<ActionResult> PatchSettingsAsync(string userName, UserSettings model)
        {
            ApplicationUser user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return NotFound();
            }

            string userId = await _userManager.GetUserIdAsync(user);
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound();
            }

            string currentUserId = _userManager.GetUserId(User);
            if (!userId.Equals(currentUserId, StringComparison.OrdinalIgnoreCase)
                && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            await _userSettingsProvider.PatchAsync(userId, model);

            return Ok();
        }

        [Route("{userName}/logins")]
        [HttpGet]
        public async Task<ActionResult<UserLogins>> GetLoginsAsync(string userName)
        {
            ApplicationUser user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return NotFound();
            }

            string userId = await _userManager.GetUserIdAsync(user);
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound();
            }

            string currentUserId = _userManager.GetUserId(User);
            if (!userId.Equals(currentUserId, StringComparison.OrdinalIgnoreCase)
                && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return new UserLogins
            {
                HasPassword = await _userManager.HasPasswordAsync(user),
                ExternalLogins = (await _userManager.GetLoginsAsync(user))
                    .Select(loginInfo => new ExternalLogin { ProviderName = loginInfo.LoginProvider, ExternalUserId = loginInfo.ProviderKey })
                    .ToList(),
            };
        }

        [Route("{userName}/logins")]
        [HttpDelete]
        public async Task<ActionResult> RemoveLoginAsync(string userName, ExternalLogin model)
        {
            ApplicationUser user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return NotFound();
            }

            string userId = await _userManager.GetUserIdAsync(user);
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound();
            }

            string currentUserId = _userManager.GetUserId(User);
            if (!userId.Equals(currentUserId, StringComparison.OrdinalIgnoreCase)
                && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            IdentityResult result = await _userManager.RemoveLoginAsync(user, model.ProviderName, model.ExternalUserId);
            if (!result.Succeeded)
            {
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return BadRequest(ModelState);
            }

            return Ok();
        }

        [Route("{userName}/setpassword")]
        [HttpPost]
        public async Task<ActionResult> SetPasswordAsync(string userName, SetPasswordRequest model)
        {
            ApplicationUser user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return NotFound();
            }

            string userId = await _userManager.GetUserIdAsync(user);
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound();
            }

            string currentUserId = _userManager.GetUserId(User);
            if (!userId.Equals(currentUserId, StringComparison.OrdinalIgnoreCase)
                && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            IdentityResult result = await _userManager.AddPasswordAsync(user, model.NewPassword);
            if (!result.Succeeded)
            {
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return BadRequest(ModelState);
            }

            return Ok();
        }

        [Route("{userName}/changepassword")]
        [HttpPost]
        public async Task<ActionResult> ChangePasswordAsync(string userName, ChangePasswordRequest model)
        {
            ApplicationUser user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return NotFound();
            }

            string userId = await _userManager.GetUserIdAsync(user);
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound();
            }

            string currentUserId = _userManager.GetUserId(User);
            if (!userId.Equals(currentUserId, StringComparison.OrdinalIgnoreCase)
                && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            IdentityResult result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return BadRequest(ModelState);
            }

            return Ok();
        }

        [Route("resetpassword")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> ResetPasswordAsync(ResetPasswordRequest model)
        {
            // Using email address since the username is public information
            ApplicationUser user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return Ok();
            }

            string code = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _emailSender.SendEmailAsync(
                model.Email,
                "Password Reset",
                $"There was a request to reset your Clicker Heroes Tracker password. If this was not you, please ignore this email.<br /><br />To reset your password, please enter this verification code:<br /><br />{code}");

            return Ok();
        }

        [Route("resetpasswordconfirmation")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> ResetPasswordConfirmationAsync(ResetPasswordConfirmationRequest model)
        {
            // Using email address since the username is public information
            ApplicationUser user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return Ok();
            }

            IdentityResult result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                return Ok();
            }

            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return BadRequest(ModelState);
        }

        internal static class ParameterConstants
        {
            internal static class Uploads
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

            internal static class Progress
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
