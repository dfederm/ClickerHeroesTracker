// <copyright file="IDatabaseCommand.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Services.Database
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a single command to a database
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
