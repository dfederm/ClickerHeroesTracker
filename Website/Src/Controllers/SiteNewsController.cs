// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System;
using System.Threading.Tasks;
using ClickerHeroesTrackerWebsite.Models.Api.SiteNews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Validation.AspNetCore;
using Website.Services.SiteNews;

namespace ClickerHeroesTrackerWebsite.Controllers
{
    /// <summary>
    /// This controller handles the set of APIs that manage site news.
    /// </summary>
    [Route("api/news")]
    [ApiController]
    public sealed class SiteNewsController : Controller
    {
        private readonly ISiteNewsProvider _siteNewsProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="SiteNewsController"/> class.
        /// </summary>
        /// <param name="siteNewsProvider">The site news provider.</param>
        public SiteNewsController(ISiteNewsProvider siteNewsProvider)
        {
            _siteNewsProvider = siteNewsProvider;
        }

        /// <summary>
        /// Gets the news entities.
        /// </summary>
        /// <returns>A response with the schema <see cref="SiteNewsEntryListResponse"/>.</returns>
        [Route("")]
        [HttpGet]
        public async Task<ActionResult<SiteNewsEntryListResponse>> ListAsync() => new SiteNewsEntryListResponse
        {
            Entries = await _siteNewsProvider.RetrieveSiteNewsEntriesAsync(),
        };

        /// <summary>
        /// Post a news entity.
        /// </summary>
        /// <param name="entry">The news entry.</param>
        /// <returns>A status code representing the result.</returns>
        [Route("")]
        [HttpPost]
        [Authorize(Roles = "Admin", AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        public async Task<ActionResult> PostAsync(SiteNewsEntry entry)
        {
            await _siteNewsProvider.AddSiteNewsEntriesAsync(entry.Date, entry.Messages);
            return StatusCode(201);
        }

        /// <summary>
        /// Delete a news entity.
        /// </summary>
        /// <param name="date">The date to delete news entries from.</param>
        /// <returns>A status code representing the result.</returns>
        [Route("{date}")]
        [HttpDelete]
        [Authorize(Roles = "Admin", AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        public async Task DeleteAsync(DateTime date) => await _siteNewsProvider.DeleteSiteNewsForDateAsync(date);
    }
}