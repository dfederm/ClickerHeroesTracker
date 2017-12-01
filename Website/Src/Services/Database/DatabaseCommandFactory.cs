// <copyright file="DatabaseCommandFactory.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Services.Database
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.SqlClient;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// A SQL command provider for the default connection string
    /// </summary>
    public sealed class DatabaseCommandFactory : IDisposable, IDatabaseCommandFactory
    {
        private readonly DatabaseSettings databaseSettings;

        private DbConnection connection;

        public DatabaseCommandFactory(IOptions<DatabaseSettings> databaseSettingsOptions)
        {
            this.databaseSettings = databaseSettingsOptions.Value;
        }

        /// <inheritdoc/>
        public IDatabaseCommand Create()
        {
            // Create the connection if it hasn't been created yet.
            if (this.connection == null)
            {
                this.connection = new SqlConnection(this.databaseSettings.ConnectionString);
                this.connection.Open();
            }

            return new DatabaseCommand(this.connection);
        }

        public void Dispose()
        {
            if (this.connection != null)
            {
                this.connection.Dispose();
                this.connection = null;
            }
        }

        private static DbConnection CreateSqlConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }
    }
}