// <copyright file="DatabaseSchemas.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using ClickerHeroesTrackerWebsite.Models;
    using ClickerHeroesTrackerWebsite.Services.Database;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Ensures the database schemas are created
    /// </summary>
    public partial class Startup
    {
        /// <summary>
        /// Ensure the database schemas are created.
        /// </summary>
        /// <remarks>
        /// This is mostly for the in-memory database used in dev mode, but could be useful for new databases too.
        /// However, it's important to note that the tables created here may not be optimized. Indicies, foreign keys, etc may be missing.
        /// </remarks>
        public void EnsureDatabaseCreated(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var serviceProvider = serviceScope.ServiceProvider;

                // Handle the EntityFramework tables
                serviceProvider.GetService<ApplicationDbContext>().Database.EnsureCreated();

                var databaseCommandFactory = serviceProvider.GetService<IDatabaseCommandFactory>();
                var databaseSettingsOptions = serviceProvider.GetService<IOptions<DatabaseSettings>>();

                // Get all existing tables so we know what already exists
                string tableNamesCommand;
                switch (databaseSettingsOptions.Value?.Kind)
                {
                    case "SqlServer":
                    {
                        tableNamesCommand = "SELECT Name FROM sys.Tables WHERE Type = N'U'";
                        break;
                    }
                    case "Sqlite":
                    {
                        tableNamesCommand = "SELECT name AS Name FROM sqlite_master WHERE type='table'";
                        break;
                    }
                    default:
                    {
                        throw new InvalidOperationException($"Invalid configuration for \"Database:Kind\": {databaseSettingsOptions.Value?.Kind}");
                    }
                }

                var existingTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                using (var command = databaseCommandFactory.Create(tableNamesCommand))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            existingTables.Add(reader["Name"].ToString());
                        }
                    }
                }

                // Read sql files and execute their contents in order if required.
                var tables = new[] {
                    "Uploads",
                    "AncientLevels",
                    "ComputedStats",
                    "OutsiderLevels",
                    "UserFollows",
                    "UserSettings",
                    "Clans",
                };
                var tableFiles = tables.Select(table => Path.Combine(this.Environment.ContentRootPath, @"Services\Database\Schemas", databaseSettingsOptions.Value.Kind, table + ".sql"));
                foreach (var tableFile in tableFiles)
                {
                    var tableName = Path.GetFileNameWithoutExtension(tableFile);
                    if (!existingTables.Contains(tableName))
                    {
                        var tableCreateCommand = File.ReadAllText(tableFile);
                        using (var command = databaseCommandFactory.Create(tableCreateCommand))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }
}
