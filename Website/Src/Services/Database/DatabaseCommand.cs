// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace ClickerHeroesTrackerWebsite.Services.Database
{
    internal sealed class DatabaseCommand : IDatabaseCommand
    {
        private readonly string _connectionString;

        private SqlConnection _connection;

        private SqlCommand _command;

        private SqlTransaction _transaction;

        public DatabaseCommand(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException("Value cannot be null or empty", nameof(connectionString));
            }

            _connectionString = connectionString;
        }

        public string CommandText { get; set; }

        public IDictionary<string, object> Parameters { get; set; }

        public async Task BeginTransactionAsync()
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("This command has already begun a transaction");
            }

            await EnsureCommandCreatedAsync();
            _transaction = _connection.BeginTransaction();
            _command.Transaction = _transaction;
        }

        public void CommitTransaction()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("This command hasn't begun a transaction");
            }

            try
            {
                _transaction.Commit();
            }
            catch (Exception)
            {
                _transaction.Rollback();
                throw;
            }
        }

        public async Task ExecuteNonQueryAsync()
        {
            await PrepareForExecutionAsync();
            await _command.ExecuteNonQueryAsync();
        }

        public async Task<object> ExecuteScalarAsync()
        {
            await PrepareForExecutionAsync();
            return await _command.ExecuteScalarAsync();
        }

        public async Task<IDataReader> ExecuteReaderAsync()
        {
            await PrepareForExecutionAsync();
            return await _command.ExecuteReaderAsync();
        }

        public void Dispose()
        {
            if (_transaction != null)
            {
                _transaction.Dispose();
                _transaction = null;
            }

            if (_command != null)
            {
                _command.Dispose();
                _command = null;
            }

            if (_connection != null)
            {
                _connection.Dispose();
                _connection = null;
            }
        }

        private async Task PrepareForExecutionAsync()
        {
            if (string.IsNullOrEmpty(CommandText))
            {
                throw new InvalidOperationException("CommandText may not be empty");
            }

            await EnsureCommandCreatedAsync();
            _command.CommandText = CommandText;
            _command.Parameters.Clear();
            if (Parameters != null)
            {
                foreach (KeyValuePair<string, object> parameter in Parameters)
                {
                    SqlParameter dbParameter = _command.CreateParameter();
                    dbParameter.ParameterName = parameter.Key;
                    dbParameter.Value = parameter.Value ?? DBNull.Value;
                    _command.Parameters.Add(dbParameter);
                }
            }
        }

        private async Task EnsureCommandCreatedAsync()
        {
            if (_connection == null)
            {
                _connection = new SqlConnection(_connectionString);
                await _connection.OpenAsync();
            }

            if (_command == null)
            {
                _command = _connection.CreateCommand();
            }
        }
    }
}