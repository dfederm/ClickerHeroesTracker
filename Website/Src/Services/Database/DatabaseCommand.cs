// <copyright file="DatabaseCommand.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Services.Database
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using Microsoft.Data.Sqlite;

    internal sealed class DatabaseCommand : IDisposable, IDatabaseCommand
    {
        private DbCommand command;

        private DbTransaction transaction;

        public DatabaseCommand(DbConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            this.command = connection.CreateCommand();
        }

        public string CommandText { get; set; }

        public IDictionary<string, object> Parameters { get; set; }

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

            this.command.ExecuteNonQuery();
        }

        public object ExecuteScalar()
        {
            this.PrepareForExecution();

            return this.command.ExecuteScalar();
        }

        public IDataReader ExecuteReader()
        {
            this.PrepareForExecution();

            return this.command.ExecuteReader();
        }

        public void Dispose()
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
            if (string.IsNullOrEmpty(this.CommandText))
            {
                throw new InvalidOperationException("CommandText may not be empty");
            }

            // Compat shim for Sqlite. This is terrible!
            if (this.command is SqliteCommand)
            {
                this.CommandText = this.CommandText
                    .Replace("SCOPE_IDENTITY", "last_insert_rowid", StringComparison.OrdinalIgnoreCase);
            }

            this.command.CommandText = this.CommandText;

            this.command.Parameters.Clear();
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