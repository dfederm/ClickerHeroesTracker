// <copyright file="Startup.Database.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using ClickerHeroesTrackerWebsite.Models;
    using ClickerHeroesTrackerWebsite.Services.Database;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;

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
        /// <returns>Async task</returns>
        private async Task EnsureDatabaseCreatedAsync(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var serviceProvider = serviceScope.ServiceProvider;

                // Handle the EntityFramework tables
                await serviceProvider.GetService<ApplicationDbContext>().Database.EnsureCreatedAsync();

                var databaseCommandFactory = serviceProvider.GetService<IDatabaseCommandFactory>();

                // Get all existing tables so we know what already exists
                var existingTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                using (var command = databaseCommandFactory.Create("SELECT Name FROM sys.Tables WHERE Type = N'U'"))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            existingTables.Add(reader["Name"].ToString());
                        }
                    }
                }

                // Read sql files and execute their contents in order if required.
                var tables = new[]
                {
                    "Uploads",
                    "AncientLevels",
                    "ComputedStats",
                    "OutsiderLevels",
                    "UserFollows",
                    "UserSettings",
                    "Clans",
                    "ClanMembers",
                    "GameUsers",
                };
                var tableFiles = tables.Select(table => Path.Combine(this.environment.ContentRootPath, @"Services\Database\Schemas", table + ".sql"));
                foreach (var tableFile in tableFiles)
                {
                    var tableName = Path.GetFileNameWithoutExtension(tableFile);
                    if (!existingTables.Contains(tableName))
                    {
                        var tableCreateCommand = File.ReadAllText(tableFile);
                        using (var command = databaseCommandFactory.Create(tableCreateCommand))
                        {
                            await command.ExecuteNonQueryAsync();
                        }
                    }
                }
            }
        }
    }
}
