// <copyright file="DatabaseCommand.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Services.Database
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;
    using ClickerHeroesTrackerWebsite.Instrumentation;
    using ClickerHeroesTrackerWebsite.Utility;
    using Microsoft.Data.Sqlite;

    internal sealed class DatabaseCommand : DisposableBase, IDatabaseCommand
    {
        private readonly ICounterProvider counterProvider;

        private DbCommand command;

        private DbTransaction transaction;

        public DatabaseCommand(
            DbConnection connection,
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

            this.command = connection.CreateCommand();

            this.counterProvider = counterProvider;
        }

        public string CommandText { get; set; }

        public CommandType CommandType { get; set; } = CommandType.Text;

        public IDictionary<string, object> Parameters { get; set; }

        public void AddTableParameter(string parameterName, string tableTypeName, DataTable table)
        {
            this.EnsureNotDisposed();

            var parameter = this.command.CreateParameter();
            parameter.ParameterName = parameterName;
            parameter.Value = table;

            var sqlParameter = parameter as SqlParameter;
            if (sqlParameter != null)
            {
                sqlParameter.SqlDbType = SqlDbType.Structured;
                sqlParameter.TypeName = tableTypeName;
            }

            var sqliteParameter = parameter as SqliteParameter;
            if (sqliteParameter != null)
            {
                sqliteParameter.SqliteType = SqliteType.Blob;
            }

            this.command.Parameters.Add(parameter);
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

        public IDataReader ExecuteReader()
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

            // Compat shim for Sqlite. This is terrible!
            if (this.command is SqliteCommand)
            {
                this.CommandText = this.CommandText
                    .Replace("SCOPE_IDENTITY", "last_insert_rowid");
            }

            this.command.CommandText = this.CommandText;
            this.command.CommandType = this.CommandType;

            // Only clear the params when this is part of a transaction. This is required since
            // some usages add params via direct casts and calls to AddTableParameter.
            // BUGBUG 63 - Remove casts to SqlDatabaseCommand (and remove this conditional, always clear).
            if (this.transaction != null)
            {
                this.command.Parameters.Clear();
            }

            if (this.Parameters != null)
            {
                foreach (var parameter in this.Parameters)
                {
                    var dbParameter = this.command.CreateParameter();
                    dbParameter.ParameterName = parameter.Key;
                    dbParameter.Value = parameter.Value ?? DBNull.Value;
                    this.command.Parameters.Add(dbParameter);
                }
            }
        }
    }
}