// <copyright file="DatabaseCommand.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Services.Database
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;
    using Microsoft.Data.SqlClient;

    internal sealed class DatabaseCommand : IDatabaseCommand
    {
        private readonly string connectionString;

        private SqlConnection connection;

        private SqlCommand command;

        private SqlTransaction transaction;

        public DatabaseCommand(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException("Value cannot be null or empty", nameof(connectionString));
            }

            this.connectionString = connectionString;
        }

        public string CommandText { get; set; }

        public IDictionary<string, object> Parameters { get; set; }

        public async Task BeginTransactionAsync()
        {
            if (this.transaction != null)
            {
                throw new InvalidOperationException("This command has already begun a transaction");
            }

            await this.EnsureCommandCreated();
            this.transaction = this.connection.BeginTransaction();
            this.command.Transaction = this.transaction;
        }

        public void CommitTransaction()
        {
            if (this.transaction == null)
            {
                throw new InvalidOperationException("This command hasn't begun a transaction");
            }

            try
            {
                this.transaction.Commit();
            }
            catch (Exception)
            {
                this.transaction.Rollback();
                throw;
            }
        }

        public async Task ExecuteNonQueryAsync()
        {
            await this.PrepareForExecutionAsync();
            await this.command.ExecuteNonQueryAsync();
        }

        public async Task<object> ExecuteScalarAsync()
        {
            await this.PrepareForExecutionAsync();
            return await this.command.ExecuteScalarAsync();
        }

        public async Task<IDataReader> ExecuteReaderAsync()
        {
            await this.PrepareForExecutionAsync();
            return await this.command.ExecuteReaderAsync();
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

            if (this.connection != null)
            {
                this.connection.Dispose();
                this.connection = null;
            }
        }

        private async Task PrepareForExecutionAsync()
        {
            if (string.IsNullOrEmpty(this.CommandText))
            {
                throw new InvalidOperationException("CommandText may not be empty");
            }

            await this.EnsureCommandCreated();

#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
            this.command.CommandText = this.CommandText;
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities

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

        private async Task EnsureCommandCreated()
        {
            if (this.connection == null)
            {
                this.connection = new SqlConnection(this.connectionString);
                await this.connection.OpenAsync();
            }

            if (this.command == null)
            {
                this.command = this.connection.CreateCommand();
            }
        }
    }
}