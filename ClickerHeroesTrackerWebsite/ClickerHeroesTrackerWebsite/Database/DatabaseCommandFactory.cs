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

    public sealed class DatabaseCommandProvider : DisposableBase, IDatabaseCommandFactory
    {
        private static string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        private static Dictionary<string, object> emptyParameters = new Dictionary<string, object>(0);

        private readonly TelemetryClient telemetryClient;

        private readonly ICounterProvider counterProvider;

        private SqlConnection connection;

        public DatabaseCommandProvider(
            TelemetryClient telemetryClient,
            ICounterProvider counterProvider)
        {
            this.telemetryClient = telemetryClient;
            this.counterProvider = counterProvider;
        }

        /// <inheritdoc/>
        public IDatabaseCommand Create(string commandText)
        {
            return this.Create(commandText, CommandType.Text, emptyParameters);
        }

        /// <inheritdoc/>
        public IDatabaseCommand Create(string commandText, IDictionary<string, object> parameters)
        {
            return this.Create(commandText, CommandType.Text, parameters);
        }

        /// <inheritdoc/>
        public IDatabaseCommand Create(string commandText, CommandType commandType)
        {
            return this.Create(commandText, commandType, emptyParameters);
        }

        /// <inheritdoc/>
        public IDatabaseCommand Create(string commandText, CommandType commandType, IDictionary<string, object> parameters)
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
                commandText,
                commandType,
                parameters,
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