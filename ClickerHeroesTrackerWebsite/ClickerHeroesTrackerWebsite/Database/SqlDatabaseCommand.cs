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

        private SqlTransaction transaction;

        public SqlDatabaseCommand(
            SqlConnection connection,
            ICounterProvider counterProvider)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (counterProvider == null)
            {
                throw new ArgumentNullException("counterProvider");
            }

            this.command = new SqlCommand();
            this.command.Connection = connection;

            this.counterProvider = counterProvider;
        }

        public string CommandText { get; set; }

        public CommandType CommandType { get; set; }

        public IDictionary<string, object> Parameters { get; set; }

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

        public void BeginTransaction()
        {
            if (this.transaction != null)
            {
                throw new InvalidOperationException("This command has already begun a transaction");
            }

            this.transaction = this.command.Connection.BeginTransaction();
            this.command.Transaction = this.transaction;
        }

        public bool CommitTransaction()
        {
            if (this.transaction == null)
            {
                throw new InvalidOperationException("This command hasn't begun a transaction");
            }

            try
            {
                this.transaction.Commit();
                return true;
            }
            catch (Exception)
            {
                this.transaction.Rollback();
                return false;
            }
        }

        public void ExecuteNonQuery()
        {
            this.PrepareForExecution();

            using (this.counterProvider.Suspend(Counter.Internal))
            using (this.counterProvider.Measure(Counter.Dependency))
            {
                this.command.ExecuteNonQuery();
            }
        }

        public object ExecuteScalar()
        {
            this.PrepareForExecution();

            using (this.counterProvider.Suspend(Counter.Internal))
            using (this.counterProvider.Measure(Counter.Dependency))
            {
                return this.command.ExecuteScalar();
            }
        }

        public SqlDataReader ExecuteReader()
        {
            this.PrepareForExecution();

            using (this.counterProvider.Suspend(Counter.Internal))
            using (this.counterProvider.Measure(Counter.Dependency))
            {
                return this.command.ExecuteReader();
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            if (this.transaction != null)
            {
                this.transaction.Dispose();
                this.transaction = null;
            }

            if (this.command != null)
            {
                this.command.Dispose();
                this.command = null;
            }
        }

        private void PrepareForExecution()
        {
            this.EnsureNotDisposed();

            if (string.IsNullOrEmpty(this.CommandText))
            {
                throw new InvalidOperationException("CommandText may not be empty");
            }

            this.command.CommandText = this.CommandText;
            this.command.CommandType = this.CommandType;

            this.command.Parameters.Clear();
            foreach (var parameter in this.Parameters)
            {
                this.command.Parameters.AddWithValue(parameter.Key, parameter.Value ?? DBNull.Value);
            }
        }
    }
}