// <copyright file="SqlDatabaseCommand.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Database
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using Instrumentation;
    using Utility;

    internal sealed class SqlDatabaseCommand : DisposableBase, IDatabaseCommand
    {
        private readonly ICounterProvider counterProvider;

        private SqlCommand command;

        public SqlDatabaseCommand(
            SqlConnection connection,
            string commandText,
            CommandType commandType,
            IDictionary<string, object> parameters,
            ICounterProvider counterProvider)
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

            if (counterProvider == null)
            {
                throw new ArgumentNullException("counterProvider");
            }

            this.command = new SqlCommand(commandText, connection);
            this.command.CommandType = commandType;

            foreach (var parameter in parameters)
            {
                this.command.Parameters.AddWithValue(parameter.Key, parameter.Value);
            }

            this.counterProvider = counterProvider;
        }

        public void AddParameter(string parameterName, object value)
        {
            this.EnsureNotDisposed();

            this.command.Parameters.AddWithValue(parameterName, value);
        }

        public void AddTableParameter(string parameterName, string tableTypeName, DataTable table)
        {
            this.EnsureNotDisposed();

            var parameter = this.command.Parameters.AddWithValue(parameterName, table);
            parameter.SqlDbType = SqlDbType.Structured;
            parameter.TypeName = tableTypeName;
        }

        public SqlParameter AddReturnParameter()
        {
            this.EnsureNotDisposed();

            var returnParameter = this.command.Parameters.Add("RetVal", SqlDbType.Int);
            returnParameter.Direction = ParameterDirection.ReturnValue;
            return returnParameter;
        }

        public void ExecuteNonQuery()
        {
            this.EnsureNotDisposed();

            using (this.counterProvider.Suspend(Counter.Internal))
            using (this.counterProvider.Measure(Counter.Dependency))
            {
                    this.command.ExecuteNonQuery();
            }
        }

        public SqlDataReader ExecuteReader()
        {
            this.EnsureNotDisposed();

            using (this.counterProvider.Suspend(Counter.Internal))
            using (this.counterProvider.Measure(Counter.Dependency))
            {
                return this.command.ExecuteReader();
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            if (this.command != null)
            {
                this.command.Dispose();
                this.command = null;
            }
        }
    }
}