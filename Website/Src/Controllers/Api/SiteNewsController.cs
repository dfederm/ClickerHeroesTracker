// <copyright file="SiteNewsController.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Controllers.Api
{
    using System;
    using System.Threading.Tasks;
    using ClickerHeroesTrackerWebsite.Models.Api.SiteNews;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Website.Services.SiteNews;

    /// <summary>
    /// This controller handles the set of APIs that manage site news
    /// </summary>
    [Route("api/news")]
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
        public async Task<IActionResult> List()
        {
            var entries = await this.siteNewsProvider.RetrieveSiteNewsEntriesAsync();
            var model = new SiteNewsEntryListResponse
            {
                Entries = entries,
            };

            return this.Ok(model);
        }

        /// <summary>
        /// Post a news entity
        /// </summary>
        /// <param name="entry">The news entry</param>
        /// <returns>A status code representing the result</returns>
        [Route("")]
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Post(SiteNewsEntry entry)
        {
            if (entry == null
                || entry.Messages == null
                || entry.Messages.Count == 0)
            {
                return this.BadRequest();
            }

            await this.siteNewsProvider.AddSiteNewsEntriesAsync(entry.Date, entry.Messages);
            return this.StatusCode(200);
        }

        /// <summary>
        /// Delete a news entity
        /// </summary>
        /// <param name="date">The date to delete news entries from</param>
        /// <returns>A status code representing the result</returns>
        [Route("{date}")]
        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public async Task Delete(DateTime date)
        {
            await this.siteNewsProvider.DeleteSiteNewsForDateAsync(date);
        }
    }
}