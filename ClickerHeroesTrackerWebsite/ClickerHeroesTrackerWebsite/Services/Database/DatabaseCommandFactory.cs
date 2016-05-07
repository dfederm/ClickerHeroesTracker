// <copyright file="DatabaseCommandFactory.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Database
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.SqlClient;
    using Instrumentation;
    using Microsoft.ApplicationInsights;
    using Microsoft.Data.Sqlite;
    using Microsoft.Extensions.Configuration;
    using Utility;

    /// <summary>
    /// A SQL command provider for the default connection string
    /// </summary>
    public sealed class DatabaseCommandFactory : DisposableBase, IDatabaseCommandFactory
    {
        private static Dictionary<string, Func<string, DbConnection>> connectionFactories = new Dictionary<string, Func<string, DbConnection>>(StringComparer.OrdinalIgnoreCase)
        {
            { "SqlServer", str => new SqlConnection(str) },
            { "Sqlite", str => new SqliteConnection(str) },
        };

        private readonly string databaseKind;

        private readonly string connectionString;

        private readonly TelemetryClient telemetryClient;

        private readonly ICounterProvider counterProvider;

        private DbConnection connection;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseCommandFactory"/> class.
        /// </summary>
        public DatabaseCommandFactory(
            IConfiguration configuration,
            TelemetryClient telemetryClient,
            ICounterProvider counterProvider)
        {
            this.connectionString = configuration["Database:ConnectionString"];
            this.databaseKind = configuration["Database:Kind"];
            this.telemetryClient = telemetryClient;
            this.counterProvider = counterProvider;
        }

        /// <inheritdoc/>
        public IDatabaseCommand Create()
        {
            this.EnsureNotDisposed();

            // Create the connection if it hasn't been created yet.
            if (this.connection == null)
            {
                this.telemetryClient.TrackEvent("SqlConnectionOpen");

                using (this.counterProvider.Suspend(Counter.Internal))
                using (this.counterProvider.Measure(Counter.Dependency))
                {
                    this.connection = connectionFactories[this.databaseKind](this.connectionString);
                    this.connection.Open();
                }
            }

            return new DatabaseCommand(
                this.connection,
                this.counterProvider);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool isDisposing)
        {
            if (this.connection != null)
            {
                this.telemetryClient.TrackEvent("SqlConnectionClose");

                // SqlConntection throws if it's being disposed in a finalizer
                if (isDisposing)
                {
                    this.connection.Dispose();
                }

                this.connection = null;
            }
        }

        private static DbConnection CreateSqlConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }
    }
}