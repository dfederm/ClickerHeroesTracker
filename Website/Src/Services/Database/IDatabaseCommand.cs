// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ClickerHeroesTrackerWebsite.Services.Database
{
    /// <summary>
    /// Represents a single command to a database.
    /// </summary>
    public interface IDatabaseCommand : IDisposable
    {
        string CommandText { get; set; }

        IDictionary<string, object> Parameters { get; set; }

        Task BeginTransactionAsync();

        void CommitTransaction();

        Task ExecuteNonQueryAsync();

        Task<object> ExecuteScalarAsync();

        Task<IDataReader> ExecuteReaderAsync();
    }
}
