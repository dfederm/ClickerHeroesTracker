// <copyright file="InMemorySiteNewsProvider.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace Website.Services.SiteNews
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// In memory provider for site news.
    /// This is used when there is no azure table storage setup.
    /// </summary>
    public class InMemorySiteNewsProvider : ISiteNewsProvider
    {
        private readonly IDictionary<DateTime, IList<string>> siteNewsDictionary = new Dictionary<DateTime, IList<string>>();

        /// <inheritdoc />
        public Task AddSiteNewsEntriesAsync(DateTime newsDate, IList<string> messages)
        {
            IList<string> currentMessages;
            if (!this.siteNewsDictionary.TryGetValue(newsDate, out currentMessages))
            {
                currentMessages = new List<string>(messages);
                this.siteNewsDictionary.Add(newsDate, currentMessages);
            }
            else
            {
                ((List<string>)currentMessages).AddRange(messages);
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task DeleteSiteNewsForDateAsync(DateTime newsDate)
        {
            this.siteNewsDictionary.Remove(newsDate);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task<IDictionary<DateTime, IList<string>>> RetrieveSiteNewsEntriesAsync()
        {
            // clone the result
            return Task.FromResult<IDictionary<DateTime, IList<string>>>(new Dictionary<DateTime, IList<string>>(this.siteNewsDictionary));
        }
    }
}
