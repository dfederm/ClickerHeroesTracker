// <copyright file="DatabaseCommandFactory.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Database
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;
    using Instrumentation;
    using Microsoft.ApplicationInsights;
    using Utility;

    /// <summary>
    /// A SQL command provider for the default connection string
    /// </summary>
    public sealed class DatabaseCommandFactory : DisposableBase, IDatabaseCommandFactory
    {
        private static string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        private readonly TelemetryClient telemetryClient;

        private readonly ICounterProvider counterProvider;

        private SqlConnection connection;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseCommandFactory"/> class.
        /// </summary>
        public DatabaseCommandFactory(
            TelemetryClient telemetryClient,
            ICounterProvider counterProvider)
        {
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
                    this.connection = new SqlConnection(connectionString);
                    this.connection.Open();
                }
            }

            return new SqlDatabaseCommand(
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
    }
}