// <copyright file="ClansController.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Controllers
{
    using System.Threading.Tasks;
    using ClickerHeroesTrackerWebsite.Models;
    using ClickerHeroesTrackerWebsite.Models.Api.Clans;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Website.Services.Clans;

    [Route("api/clans")]
    [Authorize]
    public class ClansController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;

        private readonly IClanManager clanManager;

        public ClansController(
            UserManager<ApplicationUser> userManager,
            IClanManager clanManager)
        {
            this.userManager = userManager;
            this.clanManager = clanManager;
        }

        [Route("")]
        [HttpGet]
        public async Task<ActionResult> GetClan()
        {
            var userId = this.userManager.GetUserId(this.User);
            var clanName = await this.clanManager.GetClanNameAsync(userId);

            // Fetch in parallel
            var clanDataTask = this.clanManager.GetClanDataAsync(clanName);
            var guildMembersTask = this.clanManager.GetGuildMembersAsync(clanName);
            await Task.WhenAll(clanDataTask, guildMembersTask);

            var clanData = clanDataTask.Result;
            if (clanData == null)
            {
                return this.NotFound();
            }

            clanData.GuildMembers = guildMembersTask.Result;
            return this.Ok(clanData);
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

            // Fetch in parallel
            var leaderboardTask = this.clanManager.FetchLeaderboardAsync(userId, page, count);
            var paginationTask = this.clanManager.FetchPaginationAsync(this.Request.Path, page, count);
            await Task.WhenAll(leaderboardTask, paginationTask);

            var model = new LeaderboardSummaryListResponse()
            {
                LeaderboardClans = leaderboardTask.Result,
                Pagination = paginationTask.Result,
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

            var messages = await this.clanManager.GetMessages(userId, count);
            if (messages == null)
            {
                return this.NoContent();
            }

            return this.Ok(messages);
        }

        [Route("messages")]
        [HttpPost]
        public async Task<IActionResult> SendMessage(string message)
        {
            var userId = this.userManager.GetUserId(this.User);
            var responseString = await this.clanManager.SendMessage(userId, message);
            if (responseString == null)
            {
                return this.NotFound();
            }

            return this.Ok(responseString);
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
