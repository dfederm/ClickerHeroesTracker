namespace ClickerHeroesTrackerWebsite.Database
{
    using Microsoft.ApplicationInsights;
    using System.Configuration;
    using System.Data.SqlClient;
    using System.Data;
    using System.Collections.Generic;
    using Utility;
    using Instrumentation;

    public sealed class DatabaseCommandProvider : DisposableBase, IDatabaseCommandFactory
    {
        private static string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        private static Dictionary<string, object> EmptyParameters = new Dictionary<string, object>(0);

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

        public IDatabaseCommand Create(string commandText)
        {
            return this.Create(commandText, CommandType.Text, EmptyParameters);
        }

        public IDatabaseCommand Create(string commandText, IDictionary<string, object> parameters)
        {
            return this.Create(commandText, CommandType.Text, parameters);
        }

        public IDatabaseCommand Create(string commandText, CommandType commandType)
        {
            return this.Create(commandText, commandType, EmptyParameters);
        }

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