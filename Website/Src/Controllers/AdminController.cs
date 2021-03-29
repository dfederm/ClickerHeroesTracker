// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClickerHeroesTrackerWebsite.Services.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Validation.AspNetCore;
using Website.Models.Api.Admin;

namespace Website.Controllers
{
    [Route("api/admin")]
    [Authorize(Roles = "Admin", AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [ApiController]
    public class AdminController : Controller
    {
        private readonly IDatabaseCommandFactory _databaseCommandFactory;

        public AdminController(IDatabaseCommandFactory databaseCommandFactory)
        {
            _databaseCommandFactory = databaseCommandFactory;
        }

        [Route("staleuploads")]
        [HttpGet]
        public async Task<ActionResult<List<int>>> StaleUploadsAsync()
        {
            const string CommandText = @"
                SELECT Id
                FROM Uploads
                WHERE UserId IS NULL
                AND UploadTime < DATEADD(day, -30, GETDATE())";
            List<int> uploadIds = new();
            using (IDatabaseCommand command = _databaseCommandFactory.Create(CommandText))
            using (System.Data.IDataReader reader = await command.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    int uploadId = Convert.ToInt32(reader["Id"]);
                    uploadIds.Add(uploadId);
                }
            }

            return uploadIds;
        }

        [Route("countinvalidauthtokens")]
        [HttpGet]
        public async Task<ActionResult<int>> CountInvalidAuthTokensAsync()
        {
            const string CommandText = @"
                SELECT COUNT(*) AS Count
                FROM OpenIddictTokens
                WHERE ExpirationDate < CAST(GETUTCDATE() AS datetimeoffset)
                OR Status <> 'valid'
                OR Status IS NULL";
            using (IDatabaseCommand command = _databaseCommandFactory.Create(CommandText))
            {
                return Convert.ToInt32(await command.ExecuteScalarAsync());
            }
        }

        [Route("pruneinvalidauthtokens")]
        [HttpPost]
        public async Task PruneInvalidAuthTokensAsync(PruneInvalidAuthTokensRequest model)
        {
            const string CommandTextFormat = @"
                DELETE TOP (@BatchSize)
                FROM OpenIddictTokens
                WHERE ExpirationDate < CAST(GETUTCDATE() AS datetimeoffset)
                OR Status <> 'valid'
                OR Status IS NULL";
            Dictionary<string, object> parameters = new()
            {
                { "BatchSize", model.BatchSize },
            };

            using (IDatabaseCommand command = _databaseCommandFactory.Create(CommandTextFormat, parameters))
            {
                await command.ExecuteNonQueryAsync();
            }
        }

        [Route("blockedclans")]
        [HttpGet]
        public async Task<ActionResult<List<string>>> BlockedClansAsync()
        {
            List<string> blockedClans = new();

            const string CommandText = @"
                SELECT Name
                FROM Clans
                WHERE IsBlocked = 1
                ORDER BY Name ASC;";
            using (IDatabaseCommand command = _databaseCommandFactory.Create(CommandText))
            using (System.Data.IDataReader reader = await command.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    string clanName = reader["Name"].ToString();
                    blockedClans.Add(clanName);
                }
            }

            return blockedClans;
        }

        [Route("blockclan")]
        [HttpPost]
        public async Task BlockClanAsync(BlockClanRequest model)
        {
            const string CommandText = @"
                UPDATE Clans
                SET IsBlocked = @IsBlocked
                WHERE Name = @Name;";
            Dictionary<string, object> parameters = new()
            {
                { "@Name", model.ClanName },
                { "@IsBlocked", model.IsBlocked },
            };
            using (IDatabaseCommand command = _databaseCommandFactory.Create(CommandText, parameters))
            {
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}