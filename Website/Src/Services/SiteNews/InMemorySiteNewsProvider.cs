// <copyright file="InMemorySiteNewsProvider.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace Website.Services.SiteNews
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// In memory provider for site news.
    /// This is used when there is no azure table storage setup.
    /// </summary>
    public class InMemorySiteNewsProvider : ISiteNewsProvider
    {
        private readonly ConcurrentDictionary<DateTime, IList<string>> siteNewsDictionary = new ConcurrentDictionary<DateTime, IList<string>>();

        /// <inheritdoc />
        public Task AddSiteNewsEntriesAsync(DateTime newsDate, IList<string> messages)
        {
            this.siteNewsDictionary.AddOrUpdate(newsDate, messages, (c, m) => messages);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task DeleteSiteNewsForDateAsync(DateTime newsDate)
        {
            this.siteNewsDictionary.TryRemove(newsDate, out _);
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
