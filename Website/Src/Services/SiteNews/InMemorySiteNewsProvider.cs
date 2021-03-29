// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Website.Services.SiteNews
{
    /// <summary>
    /// In memory provider for site news.
    /// This is used when there is no azure table storage setup.
    /// </summary>
    public class InMemorySiteNewsProvider : ISiteNewsProvider
    {
        private readonly ConcurrentDictionary<DateTime, IList<string>> _siteNewsDictionary = new();

        /// <inheritdoc />
        public Task EnsureCreatedAsync() => Task.CompletedTask;

        /// <inheritdoc />
        public Task AddSiteNewsEntriesAsync(DateTime newsDate, IList<string> messages)
        {
            _siteNewsDictionary.AddOrUpdate(newsDate, messages, (c, m) => messages);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task DeleteSiteNewsForDateAsync(DateTime newsDate)
        {
            _siteNewsDictionary.TryRemove(newsDate, out _);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task<IDictionary<DateTime, IList<string>>> RetrieveSiteNewsEntriesAsync()
        {
            // clone the result
            return Task.FromResult<IDictionary<DateTime, IList<string>>>(new Dictionary<DateTime, IList<string>>(_siteNewsDictionary));
        }
    }
}
