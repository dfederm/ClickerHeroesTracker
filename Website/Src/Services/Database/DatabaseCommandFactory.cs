// <copyright file="DatabaseCommandFactory.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Services.Database
{
    using Microsoft.Extensions.Options;

    /// <summary>
    /// A SQL command provider for the default connection string
    /// </summary>
    public sealed class DatabaseCommandFactory : IDatabaseCommandFactory
    {
        private readonly DatabaseSettings databaseSettings;

        public DatabaseCommandFactory(IOptions<DatabaseSettings> databaseSettingsOptions)
        {
            this.databaseSettings = databaseSettingsOptions.Value;
        }

        /// <inheritdoc/>
        public IDatabaseCommand Create()
        {
            return new DatabaseCommand(this.databaseSettings.ConnectionString);
        }
    }
}