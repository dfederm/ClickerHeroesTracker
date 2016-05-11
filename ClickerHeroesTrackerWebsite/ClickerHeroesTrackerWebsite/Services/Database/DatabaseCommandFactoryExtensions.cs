// <copyright file="DatabaseCommandFactoryExtensions.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Services.Database
{
    using System.Collections.Generic;
    using System.Data;

    /// <summary>
    /// Convenience extensions for <see cref="IDatabaseCommandFactory"/>.
    /// </summary>
    public static class DatabaseCommandFactoryExtensions
    {
        private static Dictionary<string, object> emptyParameters = new Dictionary<string, object>(0);

        /// <summary>
        /// Creates an <see cref="IDatabaseCommand"/> with <see cref="CommandType.Text"/> and no parameters.
        /// </summary>
        /// <param name="databaseCommandFactory">The command factory</param>
        /// <param name="commandText">The command text</param>
        /// <returns>An <see cref="IDatabaseCommand"/></returns>
        public static IDatabaseCommand Create(
            this IDatabaseCommandFactory databaseCommandFactory,
            string commandText)
        {
            return Create(databaseCommandFactory, commandText, CommandType.Text, emptyParameters);
        }

        /// <summary>
        /// Creates an <see cref="IDatabaseCommand"/> with <see cref="CommandType.Text"/> and specified parameters.
        /// </summary>
        /// <param name="databaseCommandFactory">The command factory</param>
        /// <param name="commandText">The command text</param>
        /// <param name="parameters">The parameters to the command</param>
        /// <returns>An <see cref="IDatabaseCommand"/></returns>
        public static IDatabaseCommand Create(
            this IDatabaseCommandFactory databaseCommandFactory,
            string commandText,
            IDictionary<string, object> parameters)
        {
            return Create(databaseCommandFactory, commandText, CommandType.Text, parameters);
        }

        /// <summary>
        /// Creates an <see cref="IDatabaseCommand"/> with the specified <see cref="CommandType"/> and no parameters.
        /// </summary>
        /// <param name="databaseCommandFactory">The command factory</param>
        /// <param name="commandText">The command text</param>
        /// <param name="commandType">The command type</param>
        /// <returns>An <see cref="IDatabaseCommand"/></returns>
        public static IDatabaseCommand Create(
            this IDatabaseCommandFactory databaseCommandFactory,
            string commandText,
            CommandType commandType)
        {
            return Create(databaseCommandFactory, commandText, commandType, emptyParameters);
        }

        /// <summary>
        /// Creates an <see cref="IDatabaseCommand"/> with the specified <see cref="CommandType"/> and no parameters.
        /// </summary>
        /// <param name="databaseCommandFactory">The command factory</param>
        /// <param name="commandText">The command text</param>
        /// <param name="commandType">The command type</param>
        /// <param name="parameters">The parameters to the command</param>
        /// <returns>An <see cref="IDatabaseCommand"/></returns>
        public static IDatabaseCommand Create(
            this IDatabaseCommandFactory databaseCommandFactory,
            string commandText,
            CommandType commandType,
            IDictionary<string, object> parameters)
        {
            var command = databaseCommandFactory.Create();
            command.CommandText = commandText;
            command.CommandType = commandType;
            command.Parameters = parameters;
            return command;
        }
    }
}