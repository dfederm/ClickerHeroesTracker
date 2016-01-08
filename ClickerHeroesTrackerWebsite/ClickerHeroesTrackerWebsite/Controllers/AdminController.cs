// <copyright file="AdminController.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Web.Mvc;
    using Database;
    using Instrumentation;
    using Microsoft.ApplicationInsights;
    using Models.Calculator;
    using Models.Game;
    using Models.SaveData;

    /// <summary>
    /// The Admin controller is where Admin users can manage the site.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly GameData gameData;

        private readonly IDatabaseCommandFactory databaseCommandFactory;

        private readonly TelemetryClient telemetryClient;

        private readonly ICounterProvider counterProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminController"/> class.
        /// </summary>
        public AdminController(
            GameData gameData,
            IDatabaseCommandFactory databaseCommandFactory,
            TelemetryClient telemetryClient,
            ICounterProvider counterProvider)
        {
            this.gameData = gameData;
            this.databaseCommandFactory = databaseCommandFactory;
            this.telemetryClient = telemetryClient;
            this.counterProvider = counterProvider;
        }

        /// <summary>
        /// GET: Admin
        /// </summary>
        /// <returns>The admin homepage view</returns>
        public ActionResult Index()
        {
            return this.View();
        }

        /// <summary>
        /// GET: UpdateComputedStats
        /// </summary>
        /// <param name="uploadIds">The upload ids to recomute stats for</param>
        /// <returns>The admin homepage view</returns>
        public ActionResult UpdateComputedStats(string uploadIds)
        {
            DataTable computedStatsTable = new DataTable();
            computedStatsTable.Columns.Add("UploadId", typeof(int));
            computedStatsTable.Columns.Add("OptimalLevel", typeof(short));
            computedStatsTable.Columns.Add("SoulsPerHour", typeof(long));
            computedStatsTable.Columns.Add("SoulsPerAscension", typeof(long));
            computedStatsTable.Columns.Add("AscensionTime", typeof(short));
            computedStatsTable.Columns.Add("TitanDamange", typeof(long));
            computedStatsTable.Columns.Add("SoulsSpent", typeof(long));

            DataTable ancientLevelsTable = new DataTable();
            ancientLevelsTable.Columns.Add("UploadId", typeof(int));
            ancientLevelsTable.Columns.Add("AncientId", typeof(byte));
            ancientLevelsTable.Columns.Add("Level", typeof(long));

            if (uploadIds != null)
            {
                if (uploadIds.Equals("ALL", StringComparison.OrdinalIgnoreCase))
                {
                    using (var command = this.databaseCommandFactory.Create("GetAllUploadContent", CommandType.StoredProcedure))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var uploadId = Convert.ToInt32(reader["Id"]);
                            var uploadContent = reader["UploadContent"].ToString();
                            this.AddRows(computedStatsTable, ancientLevelsTable, uploadId, uploadContent);
                        }
                    }
                }
                else
                {
                    var uploadIdsRaw = uploadIds.Split(',');
                    foreach (var uploadIdRaw in uploadIdsRaw)
                    {
                        int uploadId;
                        if (int.TryParse(uploadIdRaw.Trim(), out uploadId))
                        {
                            var commandParameters = new Dictionary<string, object>
                            {
                                { "@UploadId", uploadId },
                            };
                            using (var command = this.databaseCommandFactory.Create(
                                "GetUploadDetails",
                                CommandType.StoredProcedure,
                                commandParameters))
                            using (var reader = command.ExecuteReader())
                            {
                                // General upload data
                                reader.Read();
                                var uploadContent = reader["UploadContent"].ToString();

                                this.AddRows(computedStatsTable, ancientLevelsTable, uploadId, uploadContent);
                            }
                        }
                    }
                }
            }

            if (computedStatsTable.Rows.Count == 0 || ancientLevelsTable.Rows.Count == 0)
            {
                this.ViewBag.Error = "No valid upload ids";
                return this.View("Index");
            }

            using (var updateCommand = this.databaseCommandFactory.Create("UpdateUploadData", CommandType.StoredProcedure))
            {
                // BUGBUG 63 - Remove casts to SqlDatabaseCommand
                ((SqlDatabaseCommand)updateCommand).AddTableParameter("@ComputedStatsUpdates", "ComputedStatsUpdate", computedStatsTable);
                ((SqlDatabaseCommand)updateCommand).AddTableParameter("@AncientLevelsUpdates", "AncientLevelsUpdate", ancientLevelsTable);

                updateCommand.ExecuteNonQuery();
            }

            this.ViewBag.Message = "Updated " + computedStatsTable.Rows.Count + " uploads";
            return this.View("Index");
        }

        private void AddRows(
            DataTable computedStatsTable,
            DataTable ancientLevelsTable,
            int uploadId,
            string uploadContent)
        {
            var savedGame = SavedGame.Parse(uploadContent);
            if (savedGame == null)
            {
                return;
            }

            var computedStats = new ComputedStatsViewModel(
                this.gameData,
                savedGame,
                null,
                this.counterProvider);
            computedStatsTable.Rows.Add(
                uploadId,
                computedStats.OptimalLevel,
                computedStats.SoulsPerHour,
                computedStats.OptimalSoulsPerAscension,
                computedStats.OptimalAscensionTime,
                computedStats.TitanDamage,
                computedStats.SoulsSpent);

            var ancientLevels = new AncientLevelSummaryViewModel(
                this.gameData,
                savedGame.AncientsData,
                this.telemetryClient);
            foreach (var ancientLevel in ancientLevels.AncientLevels)
            {
                ancientLevelsTable.Rows.Add(
                    uploadId,
                    ancientLevel.Key.Id,
                    ancientLevel.Value);
            }
        }
    }
}