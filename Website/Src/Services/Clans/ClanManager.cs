﻿// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ClickerHeroesTrackerWebsite.Models.Api;
using ClickerHeroesTrackerWebsite.Models.Api.Clans;
using ClickerHeroesTrackerWebsite.Services.Database;
using ClickerHeroesTrackerWebsite.Utility;
using Newtonsoft.Json;

namespace Website.Services.Clans
{
    public class ClanManager : IClanManager
    {
        private const string BaseUrl = "https://guilds.clickerheroes.com";

        private static readonly char[] MessageDelimeter = { ';' };

        private readonly IDatabaseCommandFactory _databaseCommandFactory;

        private readonly HttpClient _httpClient;

        public ClanManager(
            IDatabaseCommandFactory databaseCommandFactory,
            HttpClient httpClient)
        {
            _databaseCommandFactory = databaseCommandFactory;
            _httpClient = httpClient;
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
            Dictionary<string, object> parameters = new()
            {
                { "@UserId", userId },
            };
            using (IDatabaseCommand command = _databaseCommandFactory.Create(CommandText, parameters))
            {
                return (await command.ExecuteScalarAsync())?.ToString();
            }
        }

        public async Task<ClanData> GetClanDataAsync(string clanName)
        {
            Task<ClanData> clanDataTask = GetBasicClanDataAsync(clanName);
            Task<IReadOnlyList<GuildMember>> guildMembersTask = GetGuildMembersAsync(clanName);
            await Task.WhenAll(clanDataTask, guildMembersTask);

            ClanData clanData = clanDataTask.Result;
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
            Task updateGameUsers = UpdateGameUsersTableAsync(userId, gameUserId, passwordHash);
            try
            {
                Dictionary<string, string> parameters = new(StringComparer.OrdinalIgnoreCase)
                {
                    { "uid", gameUserId },
                    { "passwordHash", passwordHash },
                };
                FormUrlEncodedContent content = new(parameters);
                HttpResponseMessage response = await _httpClient.PostAsync(BaseUrl + "/clans/getGuildInfo.php", content);

                string responseString = await response.Content.ReadAsStringAsync();
                if (responseString.Contains("\"success\": false", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                GuildResponse clanResponse = JsonConvert.DeserializeObject<GuildResponse>(responseString);
                GuildResponseResult clan = clanResponse.Result;
                if (clan?.Guild == null)
                {
                    return;
                }

                // Wait for the clans table first since it may create a foreign key required by the clan members table.
                await UpdateClansTableAsync(clan);
                await UpdateClanMembersTableAsync(clan);
            }
            finally
            {
                // Wait for this task we started
                await updateGameUsers;
            }
        }

        public async Task<IReadOnlyList<Message>> GetMessages(string userId, int count)
        {
            // Fetch in parallel
            Task<Dictionary<string, string>> parametersTask = GetBaseParametersAsync(userId);
            Task<string> clanNameTask = GetClanNameAsync(userId);
            await Task.WhenAll(parametersTask, clanNameTask);

            Dictionary<string, string> parameters = parametersTask.Result;
            if (parameters == null)
            {
                return null;
            }

            string clanName = clanNameTask.Result;
            if (clanName == null)
            {
                return null;
            }

            parameters.Add("guildName", clanName);

            // Fetch in parallel
            Task<IDictionary<string, string>> messagesTask = Task.Run(async () =>
            {
                FormUrlEncodedContent content = new(parameters);
                HttpResponseMessage response = await _httpClient.PostAsync(BaseUrl + "/clans/getGuildMessages.php", content);
                string str = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<MessageResponse>(str)?.Result?.Messages;
            });
            Task<IReadOnlyList<GuildMember>> guildMembersTask = GetGuildMembersAsync(clanName);
            await Task.WhenAll(messagesTask, guildMembersTask);

            Dictionary<string, string> guildMembers = guildMembersTask.Result.ToDictionary(member => member.Uid, member => member.Nickname);
            IDictionary<string, string> messages = messagesTask.Result;
            if (messages == null)
            {
                return null;
            }

            List<Message> messageList = new(messages.Count);
            foreach (KeyValuePair<string, string> kvp in messages)
            {
                string[] messageSplit = kvp.Value.Split(MessageDelimeter, 2);
                messageList.Add(new Message
                {
                    Content = messageSplit[1],
                    Date = Convert.ToDouble(kvp.Key).UnixTimeStampToDateTime(),
                    Username = guildMembers.TryGetValue(messageSplit[0], out string userName) ? userName : null,
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
            Task<Dictionary<string, string>> parametersTask = GetBaseParametersAsync(userId);
            Task<string> clanNameTask = GetClanNameAsync(userId);
            await Task.WhenAll(parametersTask, clanNameTask);

            Dictionary<string, string> parameters = parametersTask.Result;
            if (parameters == null)
            {
                return null;
            }

            string clanName = clanNameTask.Result;
            if (clanName == null)
            {
                return null;
            }

            parameters.Add("guildName", clanName);
            parameters.Add("message", message);

            FormUrlEncodedContent content = new(parameters);
            HttpResponseMessage response = await _httpClient.PostAsync(BaseUrl + "/clans/sendGuildMessage.php", content);
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<IReadOnlyList<LeaderboardClan>> FetchLeaderboardAsync(string filter, string userId, int page, int count)
        {
            string clanName = await GetClanNameAsync(userId);

            List<LeaderboardClan> clans = new();
            int offset = (page - 1) * count;

            const string GetLeaderboardDataCommandText = @"
                WITH NumberedRows
                AS
                (
                    SELECT ROW_NUMBER() OVER (ORDER BY CurrentRaidLevel DESC) AS RowNumber, CurrentRaidLevel, CurrentNewRaidLevel, Name, IsBlocked
                    FROM Clans
                    WHERE IsBlocked = 0
                )
                SELECT RowNumber, Name, CurrentRaidLevel, CurrentNewRaidLevel, (SELECT COUNT(*) FROM ClanMembers WHERE ClanMembers.ClanName = Name) AS MemberCount
                FROM NumberedRows
                WHERE @Filter = '' OR Name LIKE '%' + @Filter + '%'
                ORDER BY CurrentRaidLevel DESC
                    OFFSET @Offset ROWS
                    FETCH NEXT @Count ROWS ONLY;";
            Dictionary<string, object> parameters = new()
            {
                { "@Filter", filter ?? string.Empty },
                { "@Offset", offset },
                { "@Count", count },
            };
            using (IDatabaseCommand command = _databaseCommandFactory.Create(GetLeaderboardDataCommandText, parameters))
            using (IDataReader reader = await command.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    bool isUserClan = string.Equals(clanName, reader["Name"].ToString(), StringComparison.OrdinalIgnoreCase);
                    int? currentNewRaidLevel = reader["CurrentNewRaidLevel"] == DBNull.Value ? null : Convert.ToInt32(reader["CurrentNewRaidLevel"]);

                    clans.Add(new LeaderboardClan
                    {
                        Name = reader["Name"].ToString(),
                        CurrentRaidLevel = Convert.ToInt32(reader["CurrentRaidLevel"]),
                        CurrentNewRaidLevel = currentNewRaidLevel,
                        MemberCount = Convert.ToInt32(reader["MemberCount"]),
                        Rank = Convert.ToInt32(reader["RowNumber"]),
                        IsUserClan = isUserClan,
                    });
                }
            }

            return clans;
        }

        public async Task<PaginationMetadata> FetchPaginationAsync(string pageBasePath, string filter, int page, int count)
        {
            const string GetLeaderboardCountCommandText = @"
                SELECT COUNT(*) AS TotalClans
                FROM Clans
                WHERE IsBlocked = 0
                AND @SearchString = '' OR Name LIKE '%' + @SearchString + '%'";
            Dictionary<string, object> parameters = new()
            {
                { "@SearchString", filter ?? string.Empty },
            };

            using (IDatabaseCommand command = _databaseCommandFactory.Create(GetLeaderboardCountCommandText, parameters))
            using (IDataReader reader = await command.ExecuteReaderAsync())
            {
                if (!reader.Read())
                {
                    return null;
                }

                PaginationMetadata pagination = new()
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
                    SELECT ROW_NUMBER() OVER (ORDER BY CurrentRaidLevel DESC) AS RowNumber, CurrentRaidLevel, CurrentNewRaidLevel, Name, IsBlocked
                    FROM Clans
                )
                SELECT RowNumber, CurrentRaidLevel, CurrentNewRaidLevel, @ClanName AS ClanName, IsBlocked
                FROM NumberedRows
                WHERE Name = @ClanName;";
            Dictionary<string, object> parameters = new()
            {
                { "@ClanName", clanName },
            };
            using (IDatabaseCommand command = _databaseCommandFactory.Create(CommandText, parameters))
            using (IDataReader reader = await command.ExecuteReaderAsync())
            {
                if (reader.Read())
                {
                    bool isBlocked = Convert.ToBoolean(reader["IsBlocked"]);
                    int? currentNewRaidLevel = reader["CurrentNewRaidLevel"] == DBNull.Value ? null : Convert.ToInt32(reader["CurrentNewRaidLevel"]);

                    return new ClanData
                    {
                        Rank = isBlocked ? -1 : Convert.ToInt32(reader["RowNumber"]),
                        CurrentRaidLevel = Convert.ToInt32(reader["CurrentRaidLevel"]),
                        CurrentNewRaidLevel = currentNewRaidLevel,
                        ClanName = reader["ClanName"].ToString(),
                        IsBlocked = isBlocked,
                    };
                }

                return null;
            }
        }

        private async Task<IReadOnlyList<GuildMember>> GetGuildMembersAsync(string clanName)
        {
            const string CommandText = @"
                SELECT ClanMembers.Id as Uid, Nickname, HighestZone, ChosenClass, ClassLevel, UserName
                FROM ClanMembers
                LEFT JOIN GameUsers
                ON ClanMembers.Id = GameUsers.Id
                LEFT JOIN AspNetUsers
                ON GameUsers.UserId = AspNetUsers.Id
                WHERE ClanName = @ClanName;";
            Dictionary<string, object> parameters = new()
            {
                { "@ClanName", clanName },
            };
            using (IDatabaseCommand command = _databaseCommandFactory.Create(CommandText, parameters))
            using (IDataReader reader = await command.ExecuteReaderAsync())
            {
                List<GuildMember> guildMembers = new();
                while (reader.Read())
                {
                    ClanClassType? chosenClass = reader["ChosenClass"] == DBNull.Value ? null : (ClanClassType)Convert.ToInt32(reader["ChosenClass"]);
                    int? classLevel = reader["ClassLevel"] == DBNull.Value ? null : Convert.ToInt32(reader["ClassLevel"]);

                    guildMembers.Add(new GuildMember
                    {
                        Uid = reader["Uid"].ToString(),
                        Nickname = reader["Nickname"].ToString(),
                        HighestZone = Convert.ToInt32(reader["HighestZone"]),
                        ChosenClass = chosenClass,
                        ClassLevel = classLevel,
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
                    (VALUES (@Name, @CurrentRaidLevel, @CurrentNewRaidLevel, @ClanMasterId))
                    AS Input(Name, CurrentRaidLevel, CurrentNewRaidLevel, ClanMasterId)
                ON Clans.Name = Input.Name
                WHEN MATCHED THEN
                    UPDATE
                    SET
                        CurrentRaidLevel = Input.CurrentRaidLevel,
                        CurrentNewRaidLevel = Input.CurrentNewRaidLevel,
                        ClanMasterId = Input.ClanMasterId
                WHEN NOT MATCHED THEN
                    INSERT (Name, CurrentRaidLevel, CurrentNewRaidLevel, ClanMasterId)
                    VALUES (Input.Name, Input.CurrentRaidLevel, Input.CurrentNewRaidLevel, Input.ClanMasterId);";
            Dictionary<string, object> parameters = new()
            {
                    { "@Name", clan.Guild.Name },
                    { "@CurrentRaidLevel", clan.Guild.CurrentRaidLevel },
                    { "@CurrentNewRaidLevel", clan.Guild.CurrentNewRaidLevel },
                    { "@ClanMasterId", clan.Guild.GuildMasterUid },
            };
            using (IDatabaseCommand command = _databaseCommandFactory.Create(CommandText, parameters))
            {
                await command.ExecuteNonQueryAsync();
            }
        }

        private async Task UpdateClanMembersTableAsync(GuildResponseResult clan)
        {
            List<GuildMember> clanMembers = clan.GuildMembers.Values
                .Where(member => clan.Guild.MemberUids.TryGetValue(member.Uid, out MemberType memberType) && memberType == MemberType.Member)
                .OrderByDescending(member => member.HighestZone)
                .ToList();

            /* Build a query that looks like this:
                MERGE INTO ClanMembers WITH (HOLDLOCK)
                USING
                    (VALUES (@Id0, @Nickname0, @HighestZone0, @ChosenClass0, @ClassLevel0, @ClanName), (@Id1, @Nickname1, @HighestZone1, @ChosenClass1, @ClassLevel1, @ClanName), ... )
                    AS Input(Id, Nickname, HighestZone, ChosenClass, ClassLevel, ClanName)
                ON ClanMembers.Id = Input.Id
                WHEN MATCHED THEN
                    UPDATE
                    SET
                        Nickname = Input.Nickname,
                        HighestZone = Input.HighestZone,
                        ChosenClass = Input.ChosenClass,
                        ClassLevel = Input.ClassLevel,
                        ClanName = Input.ClanName
                WHEN NOT MATCHED BY TARGET THEN
                    INSERT (Id, Nickname, HighestZone, ChosenClass, ClassLevel, ClanName)
                    VALUES (Input.Id, Input.Nickname, Input.HighestZone, Input.ChosenClass, Input.ClassLevel, Input.ClanName)
                WHEN NOT MATCHED BY SOURCE AND ClanMembers.ClanName = @ClanName THEN
                    DELETE;
            */
            StringBuilder commandText = new();
            Dictionary<string, object> parameters = new()
            {
                { "@ClanName", clan.Guild.Name },
            };

            commandText.Append(@"
                MERGE INTO ClanMembers WITH (HOLDLOCK)
                USING
                    ( VALUES ");
            bool isFirst = true;
            for (int i = 0; i < clanMembers.Count; i++)
            {
                GuildMember clanMember = clanMembers[i];

                if (!isFirst)
                {
                    commandText.Append(',');
                }

                commandText.AppendFormat("(@Id{0}, @Nickname{0}, @HighestZone{0}, @ChosenClass{0}, @ClassLevel{0}, @ClanName)", i);
                parameters.Add("@Id" + i, clanMember.Uid);
                parameters.Add("@Nickname" + i, clanMember.Nickname);
                parameters.Add("@HighestZone" + i, clanMember.HighestZone);
                parameters.Add("@ChosenClass" + i, clanMember.ChosenClass.HasValue ? (ClanClassType)clanMember.ChosenClass : null);
                parameters.Add("@ClassLevel" + i, clanMember.ClassLevel);

                isFirst = false;
            }

            commandText.Append(@"
                )
                    AS Input(Id, Nickname, HighestZone, ChosenClass, ClassLevel, ClanName)
                ON ClanMembers.Id = Input.Id
                WHEN MATCHED THEN
                    UPDATE
                    SET
                        Nickname = Input.Nickname,
                        HighestZone = Input.HighestZone,
                        ChosenClass = Input.ChosenClass,
                        ClassLevel = Input.ClassLevel,
                        ClanName = Input.ClanName
                WHEN NOT MATCHED BY TARGET THEN
                    INSERT (Id, Nickname, HighestZone, ChosenClass, ClassLevel, ClanName)
                    VALUES (Input.Id, Input.Nickname, Input.HighestZone, Input.ChosenClass, Input.ClassLevel, Input.ClanName)
                WHEN NOT MATCHED BY SOURCE AND ClanMembers.ClanName = @ClanName THEN
                    DELETE;");
            using (IDatabaseCommand command = _databaseCommandFactory.Create(commandText.ToString(), parameters))
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
            Dictionary<string, object> parameters = new()
            {
                    { "@Id", gameUserId },
                    { "@PasswordHash", passwordHash },
                    { "@UserId", userId },
            };
            using (IDatabaseCommand command = _databaseCommandFactory.Create(CommandText, parameters))
            {
                await command.ExecuteNonQueryAsync();
            }
        }

        private async Task<Dictionary<string, string>> GetBaseParametersAsync(string userId)
        {
            if (userId == null)
            {
                return null;
            }

            const string CommandText = @"
                SELECT Id, PasswordHash
                FROM GameUsers
                WHERE UserId = @UserId";
            Dictionary<string, object> parameters = new()
            {
                { "@UserId", userId },
            };
            using (IDatabaseCommand command = _databaseCommandFactory.Create(CommandText, parameters))
            using (IDataReader reader = await command.ExecuteReaderAsync())
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
