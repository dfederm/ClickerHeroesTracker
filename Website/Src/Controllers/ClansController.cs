// <copyright file="ClansController.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using ClickerHeroesTrackerWebsite.Models;
    using ClickerHeroesTrackerWebsite.Models.Api;
    using ClickerHeroesTrackerWebsite.Models.Api.Clans;
    using ClickerHeroesTrackerWebsite.Models.SaveData;
    using ClickerHeroesTrackerWebsite.Services.Database;
    using ClickerHeroesTrackerWebsite.Utility;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;

    [Route("api/clans")]
    [Authorize]
    public class ClansController : Controller
    {
        private const string BaseUrl = "http://clickerheroes-savedgames3-747864888.us-east-1.elb.amazonaws.com";

        private static readonly char[] MessageDelimeter = new[] { ';' };

        private readonly IDatabaseCommandFactory databaseCommandFactory;

        private readonly UserManager<ApplicationUser> userManager;

        private readonly HttpClient httpClient;

        public ClansController(
            IDatabaseCommandFactory databaseCommandFactory,
            UserManager<ApplicationUser> userManager,
            HttpClient httpClient)
        {
            this.databaseCommandFactory = databaseCommandFactory;
            this.userManager = userManager;
            this.httpClient = httpClient;
        }

        [Route("")]
        [HttpGet]
        public async Task<ActionResult> GetClan()
        {
            var userId = this.userManager.GetUserId(this.User);
            var parameters = await this.GetBaseParameters(userId);
            if (parameters == null)
            {
                return this.NotFound();
            }

            var clan = await this.GetClanInfomation(parameters);
            if (clan?.Guild == null)
            {
                return this.NoContent();
            }

            var guildMembers = clan.GuildMembers.Values
                .Where(member => clan.Guild.MemberUids.TryGetValue(member.Uid, out var memberType) && memberType == MemberType.Member)
                .OrderByDescending(member => member.HighestZone)
                .ToList();

            const string UpsertClanCommandText = @"
                MERGE INTO Clans WITH (HOLDLOCK)
                USING
                    (VALUES (@Name, @CurrentRaidLevel, @MemberCount))
                    AS Input(Name, CurrentRaidLevel, MemberCount)
                ON Clans.Name = Input.Name
                WHEN MATCHED THEN
                    UPDATE
                    SET
                        CurrentRaidLevel = Input.CurrentRaidLevel,
                        MemberCount = Input.MemberCount
                WHEN NOT MATCHED THEN
                    INSERT (Name, CurrentRaidLevel, MemberCount)
                    VALUES (Input.Name, Input.CurrentRaidLevel, Input.MemberCount);";
            var upsertClanCommandParameters = new Dictionary<string, object>
                {
                    { "@Name", clan.Guild.Name },
                    { "@CurrentRaidLevel", clan.Guild.CurrentRaidLevel },
                    { "@MemberCount", guildMembers.Count },
                };
            using (var command = this.databaseCommandFactory.Create(UpsertClanCommandText, upsertClanCommandParameters))
            {
                await command.ExecuteNonQueryAsync();
            }

            var rank = 0;
            const string GetLeaderboardDataCommandText = @"
                WITH NumberedRows
                AS
                (
                    SELECT Name, ROW_NUMBER() OVER (ORDER BY CurrentRaidLevel DESC) AS RowNumber
                    FROM Clans
                )
                SELECT RowNumber FROM NumberedRows WHERE Name = @Name";
            var getLeaderboardDataCommandParameters = new Dictionary<string, object>
            {
                { "@Name", clan.Guild.Name },
            };
            using (var command = this.databaseCommandFactory.Create(GetLeaderboardDataCommandText, getLeaderboardDataCommandParameters))
            {
                rank = Convert.ToInt32(await command.ExecuteScalarAsync());
            }

            var model = new ClanData
            {
                ClanName = clan.Guild.Name,
                CurrentRaidLevel = clan.Guild.CurrentRaidLevel,
                GuildMembers = guildMembers,
                Rank = rank,
            };
            return this.Ok(model);
        }

        [Route("leaderboard")]
        [HttpGet]
        public async Task<IActionResult> GetLeaderboard(
            int page = ParameterConstants.LeaderboardSummaryList.Page.Default,
            int count = ParameterConstants.LeaderboardSummaryList.Count.Default)
        {
            // Validate parameters
            if (page < ParameterConstants.LeaderboardSummaryList.Page.Min)
            {
                return this.BadRequest("Invalid parameter: page");
            }

            if (count < ParameterConstants.LeaderboardSummaryList.Count.Min
                || count > ParameterConstants.LeaderboardSummaryList.Count.Max)
            {
                return this.BadRequest("Invalid parameter: count");
            }

            // Fetch in parallel
            var paginationTask = this.FetchPaginationAsync(page, count);

            var userId = this.userManager.GetUserId(this.User);
            var parameters = await this.GetBaseParameters(userId);
            var clanName = string.Empty;
            if (parameters != null)
            {
                var clan = await this.GetClanInfomation(parameters);
                clanName = clan?.Guild?.Name ?? string.Empty;
            }

            var model = new LeaderboardSummaryListResponse()
            {
                LeaderboardClans = await this.FetchLeaderboardAsync(page, count, clanName),
                Pagination = await paginationTask,
            };

            return this.Ok(model);
        }

        [Route("messages")]
        [HttpGet]
        public async Task<IActionResult> GetMessages(int count = ParameterConstants.GetMessages.Count.Default)
        {
            // Validate parameters
            if (count < ParameterConstants.GetMessages.Count.Min
                || count > ParameterConstants.GetMessages.Count.Max)
            {
                return this.BadRequest("Invalid parameter: count");
            }

            var userId = this.userManager.GetUserId(this.User);
            var parameters = await this.GetBaseParameters(userId);
            if (parameters == null)
            {
                return this.NotFound();
            }

            var clan = await this.GetClanInfomation(parameters);
            if (clan?.Guild == null)
            {
                return this.NoContent();
            }

            parameters.Add("guildName", clan.Guild.Name);

            var content = new FormUrlEncodedContent(parameters);

            var messagesResponse = await this.httpClient.PostAsync(BaseUrl + "/clans/getGuildMessages.php", content);

            var messagesResponseString = await messagesResponse.Content.ReadAsStringAsync();

            var messages = JsonConvert.DeserializeObject<MessageResponse>(messagesResponseString);
            var messageList = new List<Message>(messages.Result.Messages.Count);
            foreach (var kvp in messages.Result.Messages)
            {
                var message = new Message();
                var messageSplit = kvp.Value.Split(MessageDelimeter, 2);
                message.Content = messageSplit[1];
                var timestamp = Convert.ToDouble(kvp.Key);
                message.Date = timestamp.UnixTimeStampToDateTime();
                var member = clan.GuildMembers.Values.FirstOrDefault(t => string.Equals(t.Uid, messageSplit[0], StringComparison.OrdinalIgnoreCase));
                message.Username = member?.Nickname;

                messageList.Add(message);
            }

            if (messageList.Count > count)
            {
                // remove that number of items from the start of the list
                messageList.RemoveRange(0, messageList.Count - count);
            }

            messageList.Reverse();
            return this.Ok(messageList);
        }

        [Route("messages")]
        [HttpPost]
        public async Task<IActionResult> SendMessage(string message, string clanName)
        {
            var userId = this.userManager.GetUserId(this.User);
            var parameters = await this.GetBaseParameters(userId);
            if (parameters == null)
            {
                return this.NotFound();
            }

            parameters.Add("guildName", clanName);
            parameters.Add("message", message);

            var content = new FormUrlEncodedContent(parameters);

            var response = await this.httpClient.PostAsync(BaseUrl + "/clans/sendGuildMessage.php", content);

            var responseString = await response.Content.ReadAsStringAsync();
            return this.Ok(responseString);
        }

        private async Task<IList<LeaderboardClan>> FetchLeaderboardAsync(int page, int count, string clanName)
        {
            var clans = new List<LeaderboardClan>();
            var offset = (page - 1) * count;

            const string getLeaderboardDataCommandText = @"
                SELECT Name, CurrentRaidLevel, MemberCount
                FROM Clans
                ORDER BY CurrentRaidLevel DESC
                    OFFSET @Offset ROWS
                    FETCH NEXT @Count ROWS ONLY;";
            var parameters = new Dictionary<string, object>
            {
                { "@Offset", offset },
                { "@Count", count },
            };
            using (var command = this.databaseCommandFactory.Create(getLeaderboardDataCommandText, parameters))
            using (var reader = await command.ExecuteReaderAsync())
            {
                var i = 1;
                while (reader.Read())
                {
                    var isUserClan = string.Equals(clanName, reader["Name"].ToString(), StringComparison.OrdinalIgnoreCase);

                    clans.Add(new LeaderboardClan
                    {
                        Name = reader["Name"].ToString(),
                        CurrentRaidLevel = Convert.ToInt32(reader["CurrentRaidLevel"]),
                        MemberCount = Convert.ToInt32(reader["MemberCount"]),
                        Rank = offset + i,
                        IsUserClan = isUserClan,
                    });
                    i++;
                }
            }

            return clans;
        }

        private async Task<Dictionary<string, string>> GetBaseParameters(string userId)
        {
            var userIdParameters = new Dictionary<string, object>
            {
                { "@UserId", userId },
            };

            const string getUploadDataCommandText = @"
                SELECT TOP(1) UploadContent
                FROM Uploads
                WHERE UserId = @UserId
                ORDER BY UploadTime DESC";
            using (var command = this.databaseCommandFactory.Create(
                getUploadDataCommandText,
                userIdParameters))
            using (var reader = await command.ExecuteReaderAsync())
            {
                if (reader.Read())
                {
                    string uploadContent = reader["UploadContent"].ToString();
                    var savedGame = SavedGame.Parse(uploadContent);
                    var uniqueId = savedGame.Object.Value<string>("uniqueId");
                    var passwordHash = savedGame.Object.Value<string>("passwordHash");

                    return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "uid", uniqueId },
                        { "passwordHash", passwordHash },
                    };
                }

                return null;
            }
        }

        private async Task<PaginationMetadata> FetchPaginationAsync(int page, int count)
        {
            const string GetLeaderboardCountCommandText = @"
                SELECT COUNT(*) AS TotalClans
                FROM Clans";

            using (var command = this.databaseCommandFactory.Create(GetLeaderboardCountCommandText))
            using (var reader = await command.ExecuteReaderAsync())
            {
                if (!reader.Read())
                {
                    return null;
                }

                var pagination = new PaginationMetadata
                {
                    Count = Convert.ToInt32(reader["TotalClans"]),
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

        private async Task<Clan> GetClanInfomation(Dictionary<string, string> parameters)
        {
            var content = new FormUrlEncodedContent(parameters);
            var response = await this.httpClient.PostAsync(BaseUrl + "/clans/getGuildInfo.php", content);

            var responseString = await response.Content.ReadAsStringAsync();

            if (!responseString.Contains("\"success\": false"))
            {
                var clanResponse = JsonConvert.DeserializeObject<ClanResponse>(responseString);

                Clan clan = clanResponse.Result;

                return clan;
            }
            else
            {
                return null;
            }
        }

        internal static class ParameterConstants
        {
            internal static class LeaderboardSummaryList
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

            internal static class GetMessages
            {
                internal static class Count
                {
                    internal const int Min = 1;

                    internal const int Max = 25;

                    internal const int Default = 10;
                }
            }
        }
    }
}
