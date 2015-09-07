namespace ClickerHeroesTrackerWebsite.Models
{
    using System;
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;
    using System.Web;

    public sealed class DatabaseCommand : IDisposable
    {
        private SqlCommand command;

        private bool isDisposed = false;

        public DatabaseCommand(string storedProcedureName)
        {
            if (string.IsNullOrEmpty(storedProcedureName))
            {
                throw new ArgumentException("value may not be emtpy", "storedProcedureName");
            }

            var connection = HttpContext.Current.Items["SqlConnection"] as SqlConnection
                    ?? (SqlConnection)(HttpContext.Current.Items["SqlConnection"] = CreateConnection());

            this.command = new SqlCommand(storedProcedureName, connection);
            this.command.CommandType = CommandType.StoredProcedure;
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