﻿// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System.Collections.Generic;

namespace ClickerHeroesTrackerWebsite.Services.Database
{
    /// <summary>
    /// Convenience extensions for <see cref="IDatabaseCommandFactory"/>.
    /// </summary>
    public static class DatabaseCommandFactoryExtensions
    {
        private static readonly Dictionary<string, object> EmptyParameters = new(0);

        /// <summary>
        /// Creates an <see cref="IDatabaseCommand"/> no parameters.
        /// </summary>
        /// <param name="databaseCommandFactory">The command factory.</param>
        /// <param name="commandText">The command text.</param>
        /// <returns>An <see cref="IDatabaseCommand"/>.</returns>
        public static IDatabaseCommand Create(
            this IDatabaseCommandFactory databaseCommandFactory,
            string commandText)
        {
            return Create(databaseCommandFactory, commandText, EmptyParameters);
        }

        /// <summary>
        /// Creates an <see cref="IDatabaseCommand"/> with the specified parameters.
        /// </summary>
        /// <param name="databaseCommandFactory">The command factory.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The parameters to the command.</param>
        /// <returns>An <see cref="IDatabaseCommand"/>.</returns>
        public static IDatabaseCommand Create(
            this IDatabaseCommandFactory databaseCommandFactory,
            string commandText,
            IDictionary<string, object> parameters)
        {
            IDatabaseCommand command = databaseCommandFactory.Create();
            command.CommandText = commandText;
            command.Parameters = parameters;
            return command;
        }
    }
}