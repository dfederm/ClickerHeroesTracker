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

        [Route("{clanName}")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> Get(string clanName)
        {
            // Validate parameters
            if (string.IsNullOrEmpty(clanName))
            {
                return this.BadRequest();
            }

            var clanData = await this.clanManager.GetClanDataAsync(clanName);
            if (clanData == null)
            {
                return this.NotFound();
            }

            return this.Ok(clanData);
        }

        [Route("")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> List(
            int page = ParameterConstants.List.Page.Default,
            int count = ParameterConstants.List.Count.Default)
        {
            // Validate parameters
            if (page < ParameterConstants.List.Page.Min)
            {
                return this.BadRequest("Invalid parameter: page");
            }

            if (count < ParameterConstants.List.Count.Min
                || count > ParameterConstants.List.Count.Max)
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
            internal static class List
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
