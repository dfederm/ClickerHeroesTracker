namespace ClickerHeroesTrackerWebsite.Controllers
{
    using Models;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Microsoft.AspNet.Identity.Owin;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;
    using System.Collections.Generic;
    using Models.SaveData;
    using Models.Calculator;
    using System.Data;
    using System;

    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        /*
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

        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }

        // GET: UpdateComputedStats
        public ActionResult UpdateComputedStats()
        {
            DataTable computedStatsTable = new DataTable();
            computedStatsTable.Columns.Add("UploadId", typeof(int));
            computedStatsTable.Columns.Add("OptimalLevel", typeof(short));
            computedStatsTable.Columns.Add("SoulsPerHour", typeof(long));
            computedStatsTable.Columns.Add("SoulsPerAscension", typeof(long));
            computedStatsTable.Columns.Add("AscensionTime", typeof(short));
            computedStatsTable.Columns.Add("TitanDamange", typeof(long));
            computedStatsTable.Columns.Add("SoulsSpent", typeof(long));

            using (var command = new DatabaseCommand("GetAllUploadContent"))
            {
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var uploadId = Convert.ToInt32(reader["Id"]);
                    var uploadContent = reader["UploadContent"].ToString();

                    var savedGame = SavedGame.Parse(uploadContent);
                    if (savedGame == null)
                    {
                        continue;
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
                }
            }

            using (var command = new DatabaseCommand("UpdateComputedStats"))
            {
                command.AddTableParameter("@ComputedStatsUpdates", "ComputedStatsUpdate", computedStatsTable);
                command.ExecuteNonQuery();
            }

            this.ViewBag.Message = "Recomputed stats for " + computedStatsTable.Rows.Count + " uploads";
            return View("Index");
        }
    }
}