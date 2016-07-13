// <copyright file="IDatabaseCommand.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Services.Database
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    /// <summary>
    /// Represents a single command to a database
    /// </summary>
    public interface IDatabaseCommand : IDisposable
    {
        /// <summary>
        /// Gets or sets the command text
        /// </summary>
        string CommandText { get; set; }

        /// <summary>
        /// Gets or sets the parameters used in the command.
        /// </summary>
        IDictionary<string, object> Parameters { get; set; }

        /// <summary>
        /// Considers this command as a transaction.
        /// </summary>
        void BeginTransaction();

        /// <summary>
        /// Commits the transaction
        /// </summary>
        /// <returns>Whether the transaction succeeded</returns>
        bool CommitTransaction();

        /// <summary>
        /// Execute the command without expecting response data.
        /// </summary>
        void ExecuteNonQuery();

        /// <summary>
        /// Execute the command expecting it to return a scalar value.
        /// </summary>
        /// <returns>The return value from the command</returns>
        object ExecuteScalar();

        /// <summary>
        /// Execute the command returning a reader with which to read the response.
        /// </summary>
        /// <returns>A reader to read the response</returns>
        IDataReader ExecuteReader();
    }
}
