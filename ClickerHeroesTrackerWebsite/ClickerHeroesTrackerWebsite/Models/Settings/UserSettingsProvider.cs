namespace ClickerHeroesTrackerWebsite.Models.Settings
{
    using Database;
    using System;
    using System.Collections.Generic;

    public class UserSettingsProvider : IUserSettingsProvider
    {
        private readonly Dictionary<string, UserSettings> cache = new Dictionary<string, UserSettings>(StringComparer.OrdinalIgnoreCase);

        private readonly IDatabaseCommandFactory databaseCommandFactory;

        public UserSettingsProvider(IDatabaseCommandFactory databaseCommandFactory)
        {
            this.databaseCommandFactory = databaseCommandFactory;
        }

        public IUserSettings Get(string userId)
        {
            // Use a cache to avoid hitting the database every time
            UserSettings settings;
            if (!this.cache.TryGetValue(userId, out settings))
            {
                settings = new UserSettings(this.databaseCommandFactory, userId);
                this.cache.Add(userId, settings);
            }

            return settings;
        }

        public void FlushChanges()
        {
            foreach (var settings in this.cache.Values)
            {
                settings.FlushChanges();
            }
        }
    }
}