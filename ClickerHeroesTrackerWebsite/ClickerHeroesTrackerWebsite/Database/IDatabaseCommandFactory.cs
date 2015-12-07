// <copyright file="IDatabaseCommandFactory.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Database
{
    using System.Collections.Generic;
    using System.Data;

    /// <summary>
    /// A factory for creating <see cref="IDatabaseCommand"/>s.
    /// </summary>
    public interface IDatabaseCommandFactory
    {
        /// <summary>
        /// Creates an <see cref="IDatabaseCommand"/> with <see cref="CommandType.Text"/> and no parameters.
        /// </summary>
        /// <param name="commandText">The command text</param>
        /// <returns>An <see cref="IDatabaseCommand"/></returns>
        IDatabaseCommand Create(string commandText);

        /// <summary>
        /// Creates an <see cref="IDatabaseCommand"/> with <see cref="CommandType.Text"/> and specified parameters.
        /// </summary>
        /// <param name="commandText">The command text</param>
        /// <param name="parameters">The parameters to the command</param>
        /// <returns>An <see cref="IDatabaseCommand"/></returns>
        IDatabaseCommand Create(string commandText, IDictionary<string, object> parameters);

        /// <summary>
        /// Creates an <see cref="IDatabaseCommand"/> with the specified <see cref="CommandType"/> and no parameters.
        /// </summary>
        /// <param name="commandText">The command text</param>
        /// <param name="commandType">The command type</param>
        /// <returns>An <see cref="IDatabaseCommand"/></returns>
        IDatabaseCommand Create(string commandText, CommandType commandType);

        /// <summary>
        /// Creates an <see cref="IDatabaseCommand"/> with the specified <see cref="CommandType"/> and no parameters.
        /// </summary>
        /// <param name="commandText">The command text</param>
        /// <param name="commandType">The command type</param>
        /// <param name="parameters">The parameters to the command</param>
        /// <returns>An <see cref="IDatabaseCommand"/></returns>
        IDatabaseCommand Create(string commandText, CommandType commandType, IDictionary<string, object> parameters);
    }
}
