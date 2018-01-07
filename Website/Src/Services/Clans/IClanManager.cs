// <copyright file="IClanManager.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace Website.Services.Clans
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ClickerHeroesTrackerWebsite.Models.Api;
    using ClickerHeroesTrackerWebsite.Models.Api.Clans;

    public interface IClanManager
    {
        Task<string> GetClanNameAsync(string userId);

        Task<ClanData> GetClanDataAsync(string clanName);

        Task<IList<GuildMember>> GetGuildMembersAsync(string clanName);

        Task UpdateClanAsync(string userId, string gameUserId, string passwordHash);

        Task<IList<Message>> GetMessages(string userId, int count);

        Task<string> SendMessage(string userId, string message);

        Task<IList<LeaderboardClan>> FetchLeaderboardAsync(string userId, int page, int count);

        Task<PaginationMetadata> FetchPaginationAsync(string pageBasePath, int page, int count);
    }
}
