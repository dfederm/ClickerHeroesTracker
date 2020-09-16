// <copyright file="SiteNewsController.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Controllers
{
    using System;
    using System.Threading.Tasks;
    using ClickerHeroesTrackerWebsite.Models.Api.SiteNews;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using OpenIddict.Validation.AspNetCore;
    using Website.Services.SiteNews;

    /// <summary>
    /// This controller handles the set of APIs that manage site news
    /// </summary>
    [Route("api/news")]
    [ApiController]
    public sealed class SiteNewsController : Controller
    {
        private readonly ISiteNewsProvider siteNewsProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="SiteNewsController"/> class.
        /// </summary>
        /// <param name="siteNewsProvider">The site news provider.</param>
        public SiteNewsController(ISiteNewsProvider siteNewsProvider)
        {
            this.siteNewsProvider = siteNewsProvider;
        }

        /// <summary>
        /// Gets the news entities
        /// </summary>
        /// <returns>A response with the schema <see cref="SiteNewsEntryListResponse"/></returns>
        [Route("")]
        [HttpGet]
        public async Task<ActionResult<SiteNewsEntryListResponse>> List() => new SiteNewsEntryListResponse
        {
            Entries = await this.siteNewsProvider.RetrieveSiteNewsEntriesAsync(),
        };

        /// <summary>
        /// Post a news entity
        /// </summary>
        /// <param name="entry">The news entry</param>
        /// <returns>A status code representing the result</returns>
        [Route("")]
        [HttpPost]
        [Authorize(Roles = "Admin", AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Post(SiteNewsEntry entry)
        {
            await this.siteNewsProvider.AddSiteNewsEntriesAsync(entry.Date, entry.Messages);
            return this.StatusCode(201);
        }

        /// <summary>
        /// Delete a news entity
        /// </summary>
        /// <param name="date">The date to delete news entries from</param>
        /// <returns>A status code representing the result</returns>
        [Route("{date}")]
        [HttpDelete]
        [Authorize(Roles = "Admin", AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        public async Task Delete(DateTime date) => await this.siteNewsProvider.DeleteSiteNewsForDateAsync(date);
    }
}