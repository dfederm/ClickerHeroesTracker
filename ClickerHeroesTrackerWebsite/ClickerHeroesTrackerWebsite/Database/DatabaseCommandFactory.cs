namespace ClickerHeroesTrackerWebsite.Database
{
    using Microsoft.ApplicationInsights;
    using System;
    using System.Configuration;
    using System.Data.SqlClient;
    using System.Data;
    using System.Collections.Generic;

    public sealed class DatabaseCommandProvider : IDatabaseCommandFactory, IDisposable
    {
        private static string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        private static Dictionary<string, object> EmptyParameters = new Dictionary<string, object>(0);

        private readonly TelemetryClient telemetryClient;

        private SqlConnection connection;

        private SqlDatabaseCommand lastCommand;

        public DatabaseCommandProvider(TelemetryClient telemetryClient)
        {
            this.telemetryClient = telemetryClient;
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
            // SQL only allows one command to be active per connection, so close any command active on this connection.
            if (this.lastCommand != null)
            {
                this.lastCommand.Dispose();
            }

            // Create the connection if it hasn't been created yet.
            if (this.connection == null)
            {
                this.telemetryClient.TrackEvent("SqlConnectionOpen");

                using (new DependencyScope())
                {
                    this.connection = new SqlConnection(connectionString);
                    this.connection.Open();
                }
            }

            var command = new SqlDatabaseCommand(
                this.connection,
                commandText,
                commandType,
                parameters);

            this.lastCommand = command;

            return command;
        }

        public void Dispose()
        {
            if (this.connection != null)
            {
                this.telemetryClient.TrackEvent("SqlConnectionClose");
                this.connection.Dispose();
                this.connection = null;
            }

            if (this.lastCommand != null)
            {
                this.lastCommand.Dispose();
                this.lastCommand = null;
            }
        }
    }
}