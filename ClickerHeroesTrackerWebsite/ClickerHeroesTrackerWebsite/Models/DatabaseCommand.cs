namespace ClickerHeroesTrackerWebsite.Models
{
    using System;
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;

    public sealed class DatabaseCommand : IDisposable
    {
        private SqlConnection connection;

        private SqlCommand command;

        private bool isDisposed = false;

        public DatabaseCommand(string storedProcedureName)
        {
            if (string.IsNullOrEmpty(storedProcedureName))
            {
                throw new ArgumentException("value may not be emtpy", "storedProcedureName");
            }

            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            this.connection = new SqlConnection(connectionString);
            this.connection.Open();

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
            this.command.ExecuteNonQuery();
        }

        public SqlDataReader ExecuteReader()
        {
            return this.command.ExecuteReader();
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                this.connection.Dispose();
                this.connection = null;

                this.command.Dispose();
                this.command = null;

                isDisposed = true;
            }
        }
    }
}