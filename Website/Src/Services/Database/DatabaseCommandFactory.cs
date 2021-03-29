// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using Microsoft.Extensions.Options;

namespace ClickerHeroesTrackerWebsite.Services.Database
{
    /// <summary>
    /// A SQL command provider for the default connection string.
    /// </summary>
    public sealed class DatabaseCommandFactory : IDatabaseCommandFactory
    {
        private readonly DatabaseSettings _databaseSettings;

        public DatabaseCommandFactory(IOptions<DatabaseSettings> databaseSettingsOptions)
        {
            _databaseSettings = databaseSettingsOptions.Value;
        }

        /// <inheritdoc/>
        public IDatabaseCommand Create() => new DatabaseCommand(_databaseSettings.ConnectionString);
    }
}