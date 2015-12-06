namespace ClickerHeroesTrackerWebsite.Controllers
{
    using Models;
    using System.Web.Mvc;
    using Models.SaveData;
    using Models.Calculator;
    using System.Data;
    using System;
    using Database;
    using System.Collections.Generic;

    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        /* Keeping here for reference, but use DI when we really need it.
        private ApplicationUserManager userManager;
        private ApplicationRoleManager roleManager;

        public ApplicationUserManager UserManager
        {
            get
            {
                return userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
        }

        public ApplicationRoleManager RoleManager
        {
            get
            {
                return roleManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationRoleManager>();
            }
        }
        */

        private readonly IDatabaseCommandFactory databaseCommandFactory;

        public AdminController(IDatabaseCommandFactory databaseCommandFactory)
        {
            this.databaseCommandFactory = databaseCommandFactory;
        }

        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }

        // GET: UpdateComputedStats
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
                            AddRows(computedStatsTable, ancientLevelsTable, uploadId, uploadContent);
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
                            using (var command = this.databaseCommandFactory.Create(
                                "GetUploadDetails",
                                CommandType.StoredProcedure,
                                new Dictionary<string, object>
                                {
                                    { "@UploadId", uploadId }
                                }))
                            using (var reader = command.ExecuteReader())
                            {
                                // General upload data
                                reader.Read();
                                var uploadContent = reader["UploadContent"].ToString();

                                AddRows(computedStatsTable, ancientLevelsTable, uploadId, uploadContent);
                            }
                        }
                    }
                }
            }

            if (computedStatsTable.Rows.Count == 0 || ancientLevelsTable.Rows.Count == 0)
            {
                this.ViewBag.Error = "No valid upload ids";
                return View("Index");
            }

            using (var updateCommand = this.databaseCommandFactory.Create("UpdateUploadData", CommandType.StoredProcedure))
            {
                // BUGBUG 63 - Remove casts to SqlDatabaseCommand
                ((SqlDatabaseCommand)updateCommand).AddTableParameter("@ComputedStatsUpdates", "ComputedStatsUpdate", computedStatsTable);
                ((SqlDatabaseCommand)updateCommand).AddTableParameter("@AncientLevelsUpdates", "AncientLevelsUpdate", ancientLevelsTable);

                updateCommand.ExecuteNonQuery();
            }

            this.ViewBag.Message = "Updated " + computedStatsTable.Rows.Count + " uploads";
            return View("Index");
        }

        private static void AddRows(
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

            var computedStats = new ComputedStatsViewModel(savedGame, null);
            computedStatsTable.Rows.Add(
                uploadId,
                computedStats.OptimalLevel,
                computedStats.SoulsPerHour,
                computedStats.OptimalSoulsPerAscension,
                computedStats.OptimalAscensionTime,
                computedStats.TitanDamage,
                computedStats.SoulsSpent);

            var ancientLevels = new AncientLevelSummaryViewModel(savedGame.AncientsData);
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