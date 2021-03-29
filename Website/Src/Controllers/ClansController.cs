// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClickerHeroesTrackerWebsite.Models;
using ClickerHeroesTrackerWebsite.Models.Api;
using ClickerHeroesTrackerWebsite.Models.Api.Clans;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Website.Services.Clans;

namespace ClickerHeroesTrackerWebsite.Controllers
{
    [Route("api/clans")]
    [Authorize]
    [ApiController]
    public class ClansController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly IClanManager _clanManager;

        public ClansController(
            UserManager<ApplicationUser> userManager,
            IClanManager clanManager)
        {
            _userManager = userManager;
            _clanManager = clanManager;
        }

        [Route("{clanName}")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ClanData>> GetAsync(string clanName)
        {
            ClanData clanData = await _clanManager.GetClanDataAsync(clanName);
            if (clanData == null)
            {
                return NotFound();
            }

            return clanData;
        }

        [Route("")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<LeaderboardSummaryListResponse>> ListAsync(
            int page = ParameterConstants.List.Page.Default,
            int count = ParameterConstants.List.Count.Default)
        {
            // Validate parameters
            if (page < ParameterConstants.List.Page.Min)
            {
                return BadRequest("Invalid parameter: page");
            }

            if (count < ParameterConstants.List.Count.Min
                || count > ParameterConstants.List.Count.Max)
            {
                return BadRequest("Invalid parameter: count");
            }

            string userId = _userManager.GetUserId(User);

            // Fetch in parallel
            Task<IList<LeaderboardClan>> leaderboardTask = _clanManager.FetchLeaderboardAsync(userId, page, count);
            Task<PaginationMetadata> paginationTask = _clanManager.FetchPaginationAsync(Request.Path, page, count);
            await Task.WhenAll(leaderboardTask, paginationTask);

            return new LeaderboardSummaryListResponse
            {
                LeaderboardClans = leaderboardTask.Result,
                Pagination = paginationTask.Result,
            };
        }

        [Route("messages")]
        [HttpGet]
        public async Task<ActionResult<List<Message>>> GetMessagesAsync(int count = ParameterConstants.GetMessages.Count.Default)
        {
            // Validate parameters
            if (count < ParameterConstants.GetMessages.Count.Min
                || count > ParameterConstants.GetMessages.Count.Max)
            {
                return BadRequest("Invalid parameter: count");
            }

            string userId = _userManager.GetUserId(User);

            IList<Message> messages = await _clanManager.GetMessages(userId, count);
            if (messages == null)
            {
                return NoContent();
            }

            return messages.ToList();
        }

        [Route("messages")]
        [HttpPost]
        public async Task<ActionResult<string>> SendMessageAsync([FromForm] string message)
        {
            string userId = _userManager.GetUserId(User);
            string responseString = await _clanManager.SendMessage(userId, message);
            if (responseString == null)
            {
                return NotFound();
            }

            return responseString;
        }

        private static class ParameterConstants
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
