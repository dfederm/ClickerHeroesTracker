// <copyright file="IDatabaseCommand.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Database
{
    using System;
    using System.Data.SqlClient;

    /// <summary>
    /// Represents a single command to a database
    /// </summary>
    public interface IDatabaseCommand : IDisposable
    {
        /// <summary>
        /// Execute the command without expecting response data.
        /// </summary>
        void ExecuteNonQuery();

        /// <summary>
        /// Execute the command returning a reader with which to read the response.
        /// </summary>
        /// <remarks>BUGBUG 59 - Remove the raw SqlDataReader usage</remarks>
        /// <returns>A reader to read the response</returns>
        SqlDataReader ExecuteReader();
    }
}
