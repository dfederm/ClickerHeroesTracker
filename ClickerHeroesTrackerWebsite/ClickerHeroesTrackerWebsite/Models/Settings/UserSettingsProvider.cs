// <copyright file="UserSettingsProvider.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Settings
{
    using System;
    using System.Collections.Generic;
    using Database;

    /// <summary>
    /// An <see cref="IUserSettingsProvider"/> implementation which uses a database as the backing store.
    /// </summary>
    public class UserSettingsProvider : IUserSettingsProvider
    {
        private readonly Dictionary<string, UserSettings> cache = new Dictionary<string, UserSettings>(StringComparer.OrdinalIgnoreCase);

        private readonly IDatabaseCommandFactory databaseCommandFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserSettingsProvider"/> class.
        /// </summary>
        public UserSettingsProvider(IDatabaseCommandFactory databaseCommandFactory)
        {
            this.databaseCommandFactory = databaseCommandFactory;
        }

        /// <inheritdoc/>
        public IUserSettings Get(string userId)
        {
            // If the user isn't logged in, use the default settings
            if (string.IsNullOrEmpty(userId))
            {
                return new UserSettings();
            }

            // Use a cache to avoid hitting the database every time
            UserSettings settings;
            if (!this.cache.TryGetValue(userId, out settings))
            {
                settings = new UserSettings(this.databaseCommandFactory, userId);
                this.cache.Add(userId, settings);
            }

            return settings;
        }

        /// <inheritdoc/>
        public void FlushChanges()
        {
            foreach (var settings in this.cache.Values)
            {
                settings.FlushChanges();
            }
        }
    }
}