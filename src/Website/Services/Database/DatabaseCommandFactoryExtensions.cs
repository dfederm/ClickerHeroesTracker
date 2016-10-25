// <copyright file="DatabaseCommandFactoryExtensions.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Services.Database
{
    using System.Collections.Generic;

    /// <summary>
    /// Convenience extensions for <see cref="IDatabaseCommandFactory"/>.
    /// </summary>
    public static class DatabaseCommandFactoryExtensions
    {
        private static Dictionary<string, object> emptyParameters = new Dictionary<string, object>(0);

        /// <summary>
        /// Creates an <see cref="IDatabaseCommand"/> no parameters.
        /// </summary>
        /// <param name="databaseCommandFactory">The command factory</param>
        /// <param name="commandText">The command text</param>
        /// <returns>An <see cref="IDatabaseCommand"/></returns>
        public static IDatabaseCommand Create(
            this IDatabaseCommandFactory databaseCommandFactory,
            string commandText)
        {
            return Create(databaseCommandFactory, commandText, emptyParameters);
        }

        /// <summary>
        /// Creates an <see cref="IDatabaseCommand"/> with the specified parameters.
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
            var command = databaseCommandFactory.Create();
            command.CommandText = commandText;
            command.Parameters = parameters;
            return command;
        }
    }
}