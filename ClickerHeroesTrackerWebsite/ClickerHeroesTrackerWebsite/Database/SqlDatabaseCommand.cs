namespace ClickerHeroesTrackerWebsite.Database
{
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;
    using System.Web;

    internal sealed class SqlDatabaseCommand : IDatabaseCommand, IDisposable
    {
        private SqlCommand command;

        private bool isDisposed = false;

        public SqlDatabaseCommand(
            SqlConnection connection,
            string commandText,
            CommandType commandType,
            IDictionary<string, object> parameters)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (string.IsNullOrEmpty(commandText))
            {
                throw new ArgumentException("value may not be empty", "commandText");
            }

            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }

            this.command = new SqlCommand(commandText, connection);
            this.command.CommandType = commandType;

            foreach (var parameter in parameters)
            {
                this.command.Parameters.AddWithValue(parameter.Key, parameter.Value);
            }
        }

        // BUGBUG 57 - Use IDatabaseCommandFactory instead
        public SqlDatabaseCommand(string storedProcedureName)
            : this(
                HttpContext.Current.Items["SqlConnection"] as SqlConnection
                    ?? (SqlConnection)(HttpContext.Current.Items["SqlConnection"] = CreateConnection()),
                storedProcedureName,
                CommandType.StoredProcedure,
                new Dictionary<string, object>(0))
        {
        }

        public void AddParameter(string parameterName, object value)
        {
            this.command.Parameters.AddWithValue(parameterName, value);
        }

        public void AddTableParameter(string parameterName, string tableTypeName, DataTable table)
        {
            var parameter = this.command.Parameters.AddWithValue(parameterName, table);
            parameter.SqlDbType = SqlDbType.Structured;
            parameter.TypeName = tableTypeName;
        }

        public SqlParameter AddReturnParameter()
        {
            var returnParameter = this.command.Parameters.Add("RetVal", SqlDbType.Int);
            returnParameter.Direction = ParameterDirection.ReturnValue;
            return returnParameter;
        }

        public void ExecuteNonQuery()
        {
            using (new DependencyScope())
            {
                this.command.ExecuteNonQuery();
            }
        }

        public SqlDataReader ExecuteReader()
        {
            using (new DependencyScope())
            {
                return this.command.ExecuteReader();
            }
        }

        public void Dispose()
        {
            if (!this.isDisposed)
            {
                this.command.Dispose();
                this.command = null;

                this.isDisposed = true;
            }
        }

        private static SqlConnection CreateConnection()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            Telemetry.Client.TrackEvent("SqlConnectionOpen");

            using (new DependencyScope())
            {
                // Although this class creates the object, we want to cache and reuse the connection for the whole request.
                // We expect DatabaseConnectionClosingFilter to close the connection.
                var connection = new SqlConnection(connectionString);
                connection.Open();
                return connection;
            }
        }
    }
}