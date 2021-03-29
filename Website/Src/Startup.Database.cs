// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClickerHeroesTrackerWebsite.Models;
using ClickerHeroesTrackerWebsite.Services.Database;
using Microsoft.Extensions.DependencyInjection;

namespace ClickerHeroesTrackerWebsite
{
    /// <summary>
    /// Ensures the database schemas are created.
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
        /// <returns>Async task.</returns>
        private async Task EnsureDatabaseCreatedAsync(IServiceProvider serviceProvider)
        {
            // Handle the EntityFramework tables
            await serviceProvider.GetService<ApplicationDbContext>().Database.EnsureCreatedAsync();

            IDatabaseCommandFactory databaseCommandFactory = serviceProvider.GetService<IDatabaseCommandFactory>();

            // Get all existing tables so we know what already exists
            HashSet<string> existingTables = new(StringComparer.OrdinalIgnoreCase);
            using (IDatabaseCommand command = databaseCommandFactory.Create("SELECT Name FROM sys.Tables WHERE Type = N'U'"))
            {
                using (System.Data.IDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        existingTables.Add(reader["Name"].ToString());
                    }
                }
            }

            // Read sql files and execute their contents in order if required.
            string[] tables = new[]
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
            IEnumerable<string> tableFiles = tables.Select(table => Path.Combine(_environment.ContentRootPath, "Services", "Database", "Schemas", table + ".sql"));
            foreach (string tableFile in tableFiles)
            {
                string tableName = Path.GetFileNameWithoutExtension(tableFile);
                if (!existingTables.Contains(tableName))
                {
                    string tableCreateCommand = File.ReadAllText(tableFile);
                    using (IDatabaseCommand command = databaseCommandFactory.Create(tableCreateCommand))
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
        }
    }
}
