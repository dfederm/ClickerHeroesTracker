// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System.Collections.Generic;
using System.Threading.Tasks;
using ClickerHeroesTrackerWebsite.Models.Api;
using ClickerHeroesTrackerWebsite.Models.Api.Clans;

namespace Website.Services.Clans
{
    public interface IClanManager
    {
        Task<string> GetClanNameAsync(string userId);

        Task<ClanData> GetClanDataAsync(string clanName);

        Task UpdateClanAsync(string userId, string gameUserId, string passwordHash);

        Task<IReadOnlyList<Message>> GetMessages(string userId, int count);

        Task<string> SendMessage(string userId, string message);

        Task<IReadOnlyList<LeaderboardClan>> FetchLeaderboardAsync(string filter, string userId, int page, int count);

        Task<PaginationMetadata> FetchPaginationAsync(string pageBasePath, string filter, int page, int count);
    }
}
