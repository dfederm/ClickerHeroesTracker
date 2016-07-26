// <copyright file="ClansApiController.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Controllers.Api
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using System.Net.Http;
    using Newtonsoft.Json.Linq;
    using Models.Api.Clans;
    using System.Web.Script.Serialization;
    using Microsoft.AspNetCore.Identity;
    using Models;
    using Models.Api.Uploads;
    using Services.Database;
    using Models.SaveData;
    using Utility;
    using System.Net;
    using Microsoft.DotNet.Cli.Utils;
    using Newtonsoft.Json;

    [Route("api/clans")]
    public class ClansApiController : Controller
    {
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
        [Authorize]
        public async Task<ActionResult> GetClan()
        {
            var userId = this.userManager.GetUserId(this.User);
            SavedGame savedGame = GetLatestSave(userId);
            
            using (var client = new HttpClient())
            {
                var guildValues = new Dictionary<string, string>
                {
                   { "uid", savedGame.UniqueId },
                   { "passwordHash", savedGame.PasswordHash }
                };
                
                var content = new FormUrlEncodedContent(guildValues);

                var response = await client.PostAsync(url + "/clans/getGuildInfo.php", content);
                
                var responseString = await response.Content.ReadAsStringAsync();
                
                var clanResponse = JsonConvert.DeserializeObject<ClanResponse>(responseString);

                var clan = clanResponse.Result;

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
                    ClanName = clanResponse.Result.Guild.Name,
                    CurrentRaidLevel = clanResponse.Result.Guild.CurrentRaidLevel,
                    GuildMembers = reindexedGuildMembers
                };

                var mesagesValues = new Dictionary<string, string>
                {
                   { "uid", savedGame.UniqueId },
                   { "passwordHash", savedGame.PasswordHash },
                   { "guildName", clan.Guild.Name }
                };

                var messagesUrl = url + "/clans/getGuildMessages.php";
                content = new FormUrlEncodedContent(mesagesValues);

                var messagesResponse = await client.PostAsync(messagesUrl, content);

                var messagesResponseString = await messagesResponse.Content.ReadAsStringAsync();
                
                MessageResponse messages = JsonConvert.DeserializeObject<MessageResponse>(messagesResponseString);
                List<Message> messageList = new List<Message>();
                foreach (var mess in messages.Result.Messages)
                {
                    Message message = new Message();
                    string[] messageSplit = mess.Value.Split(';');
                    message.Content = messageSplit[1];
                    double timestamp = Convert.ToDouble(mess.Key);
                    message.Date = timestamp.UnixTimeStampToDateTime();
                    GuildMember member = clan.GuildMembers.Values.FirstOrDefault(t => t.Uid == messageSplit[0]);
                    message.Username = member.Nickname;

                    messageList.Add(message);
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
                            SET CurrentRaidLevel=@CurrentRaidLevel
                            WHERE Name=@Name
                        END
                        ELSE
                        BEGIN
                            INSERT INTO Clans(Name, CurrentRaidLevel)
                            VALUES(@Name, @CurrentRaidLevel);
                        END";
                    command.Parameters = new Dictionary<string, object>
                    {
                        { "@Name", clan.Guild.Name },
                        { "@CurrentRaidLevel", clan.Guild.CurrentRaidLevel },
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
        public IActionResult GetLeaderboard()
        {
            var clans = new List<LeaderboardClan>();
            const string getLeaderboardDataCommandText = @"
	            SELECT TOP 20 Name, CurrentRaidLevel
                FROM Clans
                ORDER BY CurrentRaidLevel DESC";
            using (var command = this.databaseCommandFactory.Create(getLeaderboardDataCommandText))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    clans.Add(new LeaderboardClan
                    {
                        Name = reader["Name"].ToString(),
                        CurrentRaidLevel = Convert.ToInt32(reader["CurrentRaidLevel"])
                    });
                }
            }

            return this.Ok(clans);
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
    }
}
