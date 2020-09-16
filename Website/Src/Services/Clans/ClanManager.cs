// <copyright file="ClanManager.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace Website.Services.Clans
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using ClickerHeroesTrackerWebsite.Models.Api;
    using ClickerHeroesTrackerWebsite.Models.Api.Clans;
    using ClickerHeroesTrackerWebsite.Services.Database;
    using ClickerHeroesTrackerWebsite.Utility;
    using Newtonsoft.Json;

    public class ClanManager : IClanManager
    {
        private const string BaseUrl = "http://clickerheroes-savedgames3-747864888.us-east-1.elb.amazonaws.com";

        private static readonly char[] MessageDelimeter = { ';' };

        private readonly IDatabaseCommandFactory databaseCommandFactory;

        private readonly HttpClient httpClient;

        public ClanManager(
            IDatabaseCommandFactory databaseCommandFactory,
            HttpClient httpClient)
        {
            this.databaseCommandFactory = databaseCommandFactory;
            this.httpClient = httpClient;
        }

        public async Task<string> GetClanNameAsync(string userId)
        {
            if (userId == null)
            {
                return null;
            }

            const string CommandText = @"
                SELECT ClanName
                FROM ClanMembers
                WHERE Id = (
                    SELECT Id
                    FROM GameUsers
                    WHERE UserId = @UserId
                );";
            var parameters = new Dictionary<string, object>
            {
                { "@UserId", userId },
            };
            using (var command = this.databaseCommandFactory.Create(CommandText, parameters))
            {
                return (await command.ExecuteScalarAsync())?.ToString();
            }
        }

        public async Task<ClanData> GetClanDataAsync(string clanName)
        {
            var clanDataTask = this.GetBasicClanDataAsync(clanName);
            var guildMembersTask = this.GetGuildMembersAsync(clanName);
            await Task.WhenAll(clanDataTask, guildMembersTask);

            var clanData = clanDataTask.Result;
            if (clanData != null)
            {
                clanData.GuildMembers = guildMembersTask.Result;
            }

            return clanData;
        }

        public async Task UpdateClanAsync(string userId, string gameUserId, string passwordHash)
        {
            if (gameUserId == null || passwordHash == null)
            {
                return;
            }

            // Start this immediately since it does not rely on getting the clan information
            var updateGameUsers = this.UpdateGameUsersTableAsync(userId, gameUserId, passwordHash);
            try
            {
                var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "uid", gameUserId },
                    { "passwordHash", passwordHash },
                };
                var content = new FormUrlEncodedContent(parameters);
                var response = await this.httpClient.PostAsync(BaseUrl + "/clans/getGuildInfo.php", content);

                var responseString = await response.Content.ReadAsStringAsync();
                if (responseString.Contains("\"success\": false", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                var clanResponse = JsonConvert.DeserializeObject<GuildResponse>(responseString);
                var clan = clanResponse.Result;
                if (clan?.Guild == null)
                {
                    return;
                }

                // Wait for the clans table first since it may create a foreign key required by the clan members table.
                await this.UpdateClansTableAsync(clan);
                await this.UpdateClanMembersTableAsync(clan);
            }
            finally
            {
                // Wait for this task we started
                await updateGameUsers;
            }
        }

        public async Task<IList<Message>> GetMessages(string userId, int count)
        {
            // Fetch in parallel
            var parametersTask = this.GetBaseParameters(userId);
            var clanNameTask = this.GetClanNameAsync(userId);
            await Task.WhenAll(parametersTask, clanNameTask);

            var parameters = parametersTask.Result;
            if (parameters == null)
            {
                return null;
            }

            var clanName = clanNameTask.Result;
            if (clanName == null)
            {
                return null;
            }

            parameters.Add("guildName", clanName);

            // Fetch in parallel
            var messagesTask = Task.Run(async () =>
            {
                var content = new FormUrlEncodedContent(parameters);
                var response = await this.httpClient.PostAsync(BaseUrl + "/clans/getGuildMessages.php", content);
                var str = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<MessageResponse>(str)?.Result?.Messages;
            });
            var guildMembersTask = this.GetGuildMembersAsync(clanName);
            await Task.WhenAll(messagesTask, guildMembersTask);

            var guildMembers = guildMembersTask.Result.ToDictionary(member => member.Uid, member => member.Nickname);
            var messages = messagesTask.Result;
            if (messages == null)
            {
                return null;
            }

            var messageList = new List<Message>(messages.Count);
            foreach (var kvp in messages)
            {
                var messageSplit = kvp.Value.Split(MessageDelimeter, 2);
                messageList.Add(new Message
                {
                    Content = messageSplit[1],
                    Date = Convert.ToDouble(kvp.Key).UnixTimeStampToDateTime(),
                    Username = guildMembers.TryGetValue(messageSplit[0], out var userName) ? userName : null,
                });
            }

            if (messageList.Count > count)
            {
                // remove that number of items from the start of the list
                messageList.RemoveRange(0, messageList.Count - count);
            }

            messageList.Reverse();
            return messageList;
        }

        public async Task<string> SendMessage(string userId, string message)
        {
            // Fetch in parallel
            var parametersTask = this.GetBaseParameters(userId);
            var clanNameTask = this.GetClanNameAsync(userId);
            await Task.WhenAll(parametersTask, clanNameTask);

            var parameters = parametersTask.Result;
            if (parameters == null)
            {
                return null;
            }

            var clanName = clanNameTask.Result;
            if (clanName == null)
            {
                return null;
            }

            parameters.Add("guildName", clanName);
            parameters.Add("message", message);

            var content = new FormUrlEncodedContent(parameters);
            var response = await this.httpClient.PostAsync(BaseUrl + "/clans/sendGuildMessage.php", content);
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<IList<LeaderboardClan>> FetchLeaderboardAsync(string userId, int page, int count)
        {
            var clanName = await this.GetClanNameAsync(userId);

            var clans = new List<LeaderboardClan>();
            var offset = (page - 1) * count;

            const string GetLeaderboardDataCommandText = @"
                SELECT Name, CurrentRaidLevel, (SELECT COUNT(*) FROM ClanMembers WHERE ClanMembers.ClanName = Name) AS MemberCount
                FROM Clans
                WHERE IsBlocked = 0
                ORDER BY CurrentRaidLevel DESC
                    OFFSET @Offset ROWS
                    FETCH NEXT @Count ROWS ONLY;";
            var parameters = new Dictionary<string, object>
            {
                { "@Offset", offset },
                { "@Count", count },
            };
            using (var command = this.databaseCommandFactory.Create(GetLeaderboardDataCommandText, parameters))
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

        public async Task<PaginationMetadata> FetchPaginationAsync(string pageBasePath, int page, int count)
        {
            const string GetLeaderboardCountCommandText = @"
                SELECT COUNT(*) AS TotalClans
                FROM Clans
                WHERE IsBlocked = 0";

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

                if (page > 1)
                {
                    pagination.Previous = $"{pageBasePath}?{nameof(page)}={page - 1}&{nameof(count)}={count}";
                }

                if (page <= Math.Ceiling((float)pagination.Count / count))
                {
                    pagination.Next = $"{pageBasePath}?{nameof(page)}={page + 1}&{nameof(count)}={count}";
                }

                return pagination;
            }
        }

        private async Task<ClanData> GetBasicClanDataAsync(string clanName)
        {
            const string CommandText = @"
                WITH NumberedRows
                AS
                (
                    SELECT ROW_NUMBER() OVER (ORDER BY CurrentRaidLevel DESC) AS RowNumber, CurrentRaidLevel, Name, IsBlocked
                    FROM Clans
                )
                SELECT RowNumber, CurrentRaidLevel, @ClanName AS ClanName, IsBlocked
                FROM NumberedRows
                WHERE Name = @ClanName;";
            var parameters = new Dictionary<string, object>
            {
                { "@ClanName", clanName },
            };
            using (var command = this.databaseCommandFactory.Create(CommandText, parameters))
            using (var reader = await command.ExecuteReaderAsync())
            {
                if (reader.Read())
                {
                    var isBlocked = Convert.ToBoolean(reader["IsBlocked"]);

                    return new ClanData
                    {
                        Rank = isBlocked ? -1 : Convert.ToInt32(reader["RowNumber"]),
                        CurrentRaidLevel = Convert.ToInt32(reader["CurrentRaidLevel"]),
                        ClanName = reader["ClanName"].ToString(),
                        IsBlocked = isBlocked,
                    };
                }

                return null;
            }
        }

        private async Task<IList<GuildMember>> GetGuildMembersAsync(string clanName)
        {
            const string CommandText = @"
                SELECT ClanMembers.Id as Uid, Nickname, HighestZone, UserName
                FROM ClanMembers
                LEFT JOIN GameUsers
                ON ClanMembers.Id = GameUsers.Id
                LEFT JOIN AspNetUsers
                ON GameUsers.UserId = AspNetUsers.Id
                WHERE ClanName = @ClanName;";
            var parameters = new Dictionary<string, object>
            {
                { "@ClanName", clanName },
            };
            using (var command = this.databaseCommandFactory.Create(CommandText, parameters))
            using (var reader = await command.ExecuteReaderAsync())
            {
                var guildMembers = new List<GuildMember>();
                while (reader.Read())
                {
                    guildMembers.Add(new GuildMember
                    {
                        Uid = reader["Uid"].ToString(),
                        Nickname = reader["Nickname"].ToString(),
                        HighestZone = Convert.ToInt32(reader["HighestZone"]),
                        UserName = reader["UserName"]?.ToString(),
                    });
                }

                return guildMembers;
            }
        }

        private async Task UpdateClansTableAsync(GuildResponseResult clan)
        {
            const string CommandText = @"
                MERGE INTO Clans WITH (HOLDLOCK)
                USING
                    (VALUES (@Name, @CurrentRaidLevel, @ClanMasterId))
                    AS Input(Name, CurrentRaidLevel, ClanMasterId)
                ON Clans.Name = Input.Name
                WHEN MATCHED THEN
                    UPDATE
                    SET
                        CurrentRaidLevel = Input.CurrentRaidLevel,
                        ClanMasterId = Input.ClanMasterId
                WHEN NOT MATCHED THEN
                    INSERT (Name, CurrentRaidLevel, ClanMasterId)
                    VALUES (Input.Name, Input.CurrentRaidLevel, Input.ClanMasterId);";
            var parameters = new Dictionary<string, object>
                {
                    { "@Name", clan.Guild.Name },
                    { "@CurrentRaidLevel", clan.Guild.CurrentRaidLevel },
                    { "@ClanMasterId", clan.Guild.GuildMasterUid },
                };
            using (var command = this.databaseCommandFactory.Create(CommandText, parameters))
            {
                await command.ExecuteNonQueryAsync();
            }
        }

        private async Task UpdateClanMembersTableAsync(GuildResponseResult clan)
        {
            var clanMembers = clan.GuildMembers.Values
                .Where(member => clan.Guild.MemberUids.TryGetValue(member.Uid, out var memberType) && memberType == MemberType.Member)
                .OrderByDescending(member => member.HighestZone)
                .ToList();

            /* Build a query that looks like this:
                MERGE INTO ClanMembers WITH (HOLDLOCK)
                USING
                    (VALUES (@Id0, @Nickname0, @HighestZone0, @ClanName), (@Id1, @Nickname1, @HighestZone1, @ClanName), ... )
                    AS Input(Id, Nickname, HighestZone, ClanName)
                ON ClanMembers.Id = Input.Id
                WHEN MATCHED THEN
                    UPDATE
                    SET
                        Nickname = Input.Nickname,
                        HighestZone = Input.HighestZone,
                        ClanName = Input.ClanName
                WHEN NOT MATCHED BY TARGET THEN
                    INSERT (Id, Nickname, HighestZone, ClanName)
                    VALUES (Input.Id, Input.Nickname, Input.HighestZone, Input.ClanName)
                WHEN NOT MATCHED BY SOURCE AND ClanMembers.ClanName = @ClanName THEN
                    DELETE;
            */
            var commandText = new StringBuilder();
            var parameters = new Dictionary<string, object>
                {
                    { "@ClanName", clan.Guild.Name },
                };

            commandText.Append(@"
                MERGE INTO ClanMembers WITH (HOLDLOCK)
                USING
                    ( VALUES ");
            var isFirst = true;
            for (var i = 0; i < clanMembers.Count; i++)
            {
                var clanMember = clanMembers[i];

                if (!isFirst)
                {
                    commandText.Append(',');
                }

                commandText.AppendFormat("(@Id{0}, @Nickname{0}, @HighestZone{0}, @ClanName)", i);
                parameters.Add("@Id" + i, clanMember.Uid);
                parameters.Add("@Nickname" + i, clanMember.Nickname);
                parameters.Add("@HighestZone" + i, clanMember.HighestZone);

                isFirst = false;
            }

            commandText.Append(@"
                )
                    AS Input(Id, Nickname, HighestZone, ClanName)
                ON ClanMembers.Id = Input.Id
                WHEN MATCHED THEN
                    UPDATE
                    SET
                        Nickname = Input.Nickname,
                        HighestZone = Input.HighestZone,
                        ClanName = Input.ClanName
                WHEN NOT MATCHED BY TARGET THEN
                    INSERT (Id, Nickname, HighestZone, ClanName)
                    VALUES (Input.Id, Input.Nickname, Input.HighestZone, Input.ClanName)
                WHEN NOT MATCHED BY SOURCE AND ClanMembers.ClanName = @ClanName THEN
                    DELETE;");
            using (var command = this.databaseCommandFactory.Create(commandText.ToString(), parameters))
            {
                await command.ExecuteNonQueryAsync();
            }
        }

        private async Task UpdateGameUsersTableAsync(string userId, string gameUserId, string passwordHash)
        {
            if (userId == null)
            {
                return;
            }

            const string CommandText = @"
                MERGE INTO GameUsers WITH (HOLDLOCK)
                USING
                    (VALUES (@Id, @PasswordHash, @UserId))
                    AS Input(Id, PasswordHash, UserId)
                ON GameUsers.UserId = Input.UserId
                WHEN MATCHED THEN
                    UPDATE
                    SET
                        Id = Input.Id,
                        PasswordHash = Input.PasswordHash
                WHEN NOT MATCHED THEN
                    INSERT (Id, PasswordHash, UserId)
                    VALUES (Input.Id, Input.PasswordHash, Input.UserId);";
            var parameters = new Dictionary<string, object>
                {
                    { "@Id", gameUserId },
                    { "@PasswordHash", passwordHash },
                    { "@UserId", userId },
                };
            using (var command = this.databaseCommandFactory.Create(CommandText, parameters))
            {
                await command.ExecuteNonQueryAsync();
            }
        }

        private async Task<Dictionary<string, string>> GetBaseParameters(string userId)
        {
            if (userId == null)
            {
                return null;
            }

            const string CommandText = @"
                SELECT Id, PasswordHash
                FROM GameUsers
                WHERE UserId = @UserId";
            var parameters = new Dictionary<string, object>
            {
                { "@UserId", userId },
            };
            using (var command = this.databaseCommandFactory.Create(CommandText, parameters))
            using (var reader = await command.ExecuteReaderAsync())
            {
                if (reader.Read())
                {
                    return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "uid", reader["Id"].ToString() },
                        { "passwordHash", reader["PasswordHash"].ToString() },
                    };
                }

                return null;
            }
        }
    }
}
