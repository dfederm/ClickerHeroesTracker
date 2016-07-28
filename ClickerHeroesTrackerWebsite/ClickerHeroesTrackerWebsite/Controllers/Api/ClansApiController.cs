// <copyright file="ClansApiController.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Controllers.Api
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
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;

    [Route("api/clans")]
    [Authorize]
    public class ClansApiController : Controller
    {
        private static readonly char[] messageDelimeter = new[] { ';' };

        private readonly IDatabaseCommandFactory databaseCommandFactory;

        private readonly UserManager<ApplicationUser> userManager;

        private const string url = "http://clickerheroes-savedgames3-747864888.us-east-1.elb.amazonaws.com";
        
        public ClansApiController(
            IDatabaseCommandFactory databaseCommandFactory,
            UserManager<ApplicationUser> userManager)
        {
            this.databaseCommandFactory = databaseCommandFactory;
            this.userManager = userManager;
        }

        [Route("")]
        [HttpGet]
        public async Task<ActionResult> GetClan()
        {
            var userId = this.userManager.GetUserId(this.User);
            SavedGame savedGame = GetLatestSave(userId);

            if (savedGame?.UniqueId == null)
            {
                return this.NotFound();
            }

            using (var client = new HttpClient())
            {
                Clan clan = await GetClanInfomation(client, savedGame);

                if (clan == null)
                {
                    return this.NoContent();
                }

                HashSet<string> guildMemberIds = new HashSet<string>();
                foreach (var x in clan.Guild.MemberUids)
                {
                    if (x.Value == MemberType.Member)
                    {
                        guildMemberIds.Add(x.Key);
                    }
                }

                var filteredGuildMembers = clan.GuildMembers
                    .Where(kvp => guildMemberIds.Contains(kvp.Value.Uid))
                    .OrderByDescending(kvp => kvp.Value.HighestZone);
                List<GuildMember> reindexedGuildMembers = new List<GuildMember>();
                foreach (var x in filteredGuildMembers)
                {
                    reindexedGuildMembers.Add(x.Value);
                }

                ClanData clanData = new ClanData()
                {
                    ClanName = clan.Guild.Name,
                    CurrentRaidLevel = clan.Guild.CurrentRaidLevel,
                    GuildMembers = reindexedGuildMembers
                };

                var mesagesValues = new Dictionary<string, string>
                {
                   { "uid", savedGame.UniqueId },
                   { "passwordHash", savedGame.PasswordHash },
                   { "guildName", clan.Guild.Name }
                };

                var messagesUrl = url + "/clans/getGuildMessages.php";
                var content = new FormUrlEncodedContent(mesagesValues);

                var messagesResponse = await client.PostAsync(messagesUrl, content);

                var messagesResponseString = await messagesResponse.Content.ReadAsStringAsync();
                
                MessageResponse messages = JsonConvert.DeserializeObject<MessageResponse>(messagesResponseString);
                List<Message> messageList = new List<Message>();
                foreach (var mess in messages.Result.Messages)
                {
                    Message message = new Message();
                    string[] messageSplit = mess.Value.Split(messageDelimeter, 2);
                    message.Content = messageSplit[1];
                    double timestamp = Convert.ToDouble(mess.Key);
                    message.Date = timestamp.UnixTimeStampToDateTime();
                    GuildMember member = clan.GuildMembers.Values.FirstOrDefault(t => t.Uid == messageSplit[0]);
                    message.Username = member?.Nickname;

                    messageList.Add(message);
                }

                var count = messageList.Count - 15;
                if (count > 0)
                {
                    // remove that number of items from the start of the list
                    messageList.RemoveRange(0, count);
                }

                messageList.Reverse();
                clanData.Messages = messageList; 

                using (var command = this.databaseCommandFactory.Create())
                {
                    // Insert Clan
                    command.CommandText = @"
                        IF EXISTS (SELECT * FROM Clans WHERE Name = @Name)
                        BEGIN
                            UPDATE Clans
                            SET CurrentRaidLevel=@CurrentRaidLevel,MemberCount=@MemberCount
                            WHERE Name=@Name
                        END
                        ELSE
                        BEGIN
                            INSERT INTO Clans(Name, CurrentRaidLevel,MemberCount)
                            VALUES(@Name, @CurrentRaidLevel, @MemberCount);
                        END";
                    command.Parameters = new Dictionary<string, object>
                    {
                        { "@Name", clan.Guild.Name },
                        { "@CurrentRaidLevel", clan.Guild.CurrentRaidLevel },
                        { "@MemberCount", reindexedGuildMembers.Count }
                    };
                    command.ExecuteNonQuery();
                }

                return this.Ok(clanData);
            }
        }
        
        [Route("messages")]
        [HttpPost]
        public async Task<IActionResult> SendMessage(string message, string clanName)
        {
            
            var userId = this.userManager.GetUserId(this.User);
            SavedGame savedGame = GetLatestSave(userId);

            if (savedGame?.UniqueId == null)
            {
                return this.NotFound();
            }

            using (var client = new HttpClient())
            {
                var guildValues = new Dictionary<string, string>
                {
                    {"guildName", clanName},
                    {"message", message},
                    {"uid", savedGame.UniqueId},
                    {"passwordHash", savedGame.PasswordHash}
                };

                var content = new FormUrlEncodedContent(guildValues);

                var response = await client.PostAsync(url + "/clans/sendGuildMessage.php", content);
                
                var responseString = await response.Content.ReadAsStringAsync();
                return this.Ok(responseString);
            }
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

            var userId = this.userManager.GetUserId(this.User);
            SavedGame savedGame = GetLatestSave(userId);
            var clanName = "";

            if (savedGame?.UniqueId != null)
            {
                using (var client = new HttpClient())
                {
                    Clan clan = await GetClanInfomation(client, savedGame);

                    if (clan != null)
                    {
                        clanName = clan.Guild.Name;
                    }
                }
            }
            
            var model = new LeaderboardSummaryListResponse()
            {
                LeaderboardClans = this.FetchLeaderboard(page, count, clanName),
                Pagination = this.FetchPagination(page, count),
            };

            return this.Ok(model);
        }

        [Route("userClan")]
        [HttpGet]
        public async Task<IActionResult> GetUserClan()
        {
            var userId = this.userManager.GetUserId(this.User);
            SavedGame savedGame = GetLatestSave(userId);

            if (savedGame?.UniqueId == null)
            {
                return this.NoContent();
            }
            using (var client = new HttpClient())
            {
                Clan clan = await GetClanInfomation(client, savedGame);

                if (clan == null)
                {
                    return this.NoContent();
                }

                const string GetLeaderboardDataCommandText = @"
                    WITH NumberedRows
                    AS
                    (SELECT Name, CurrentRaidLevel, MemberCount, ROW_NUMBER() OVER (ORDER BY CurrentRaidLevel DESC) AS RowNumber
                    FROM Clans)
                    SELECT Name, CurrentRaidLevel, MemberCount, RowNumber FROM NumberedRows WHERE Name = @Name";
                var parameters = new Dictionary<string, object>
                {
                    { "@Name", clan.Guild.Name },
                };
                var leaderboardClan = new LeaderboardClan();
                using (var command = this.databaseCommandFactory.Create(GetLeaderboardDataCommandText, parameters))
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        leaderboardClan.Name = reader["Name"].ToString();
                        leaderboardClan.CurrentRaidLevel = Convert.ToInt32(reader["CurrentRaidLevel"]);
                        leaderboardClan.MemberCount = Convert.ToInt32(reader["MemberCount"]);
                        leaderboardClan.Rank = Convert.ToInt32(reader["RowNumber"]);
                        leaderboardClan.IsUserClan = true;
                    }
                }

                return this.Ok(leaderboardClan);
            }
        }

        public IList<LeaderboardClan> FetchLeaderboard(int page, int count, string clanName)
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
            using (var reader = command.ExecuteReader())
            {
                var i = 1;
                while (reader.Read())
                {
                    bool IsUserClan = clanName == reader["Name"].ToString();

                    clans.Add(new LeaderboardClan
                    {
                        Name = reader["Name"].ToString(),
                        CurrentRaidLevel = Convert.ToInt32(reader["CurrentRaidLevel"]),
                        MemberCount = Convert.ToInt32(reader["MemberCount"]),
                        Rank = offset + i,
                        IsUserClan = IsUserClan
                    });
                    i++;
                }
            }

            return clans;
        }

        public SavedGame GetLatestSave(string userId)
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
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    string uploadContent = reader["UploadContent"].ToString();
                    return SavedGame.Parse(uploadContent);
                }

                return null;
            }
        }

        private PaginationMetadata FetchPagination(int page, int count)
        {
            const string GetLeaderboardCountCommandText = @"
	            SELECT COUNT(*) AS TotalClans
		        FROM Clans";

            using (var command = this.databaseCommandFactory.Create(GetLeaderboardCountCommandText))
            using (var reader = command.ExecuteReader())
            {
                if (!reader.Read())
                {
                    return null;
                }

                var pagination = new PaginationMetadata
                {
                    Count = Convert.ToInt32(reader["TotalClans"])
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

        private async Task<Clan> GetClanInfomation(HttpClient client, SavedGame savedGame)
        {
            var guildValues = new Dictionary<string, string>
                {
                    {"uid", savedGame.UniqueId},
                    {"passwordHash", savedGame.PasswordHash}
                };

            var content = new FormUrlEncodedContent(guildValues);

            var response = await client.PostAsync(url + "/clans/getGuildInfo.php", content);

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
        }
    }
}
