namespace ClickerHeroesTrackerWebsite.Controllers
{
    using Models.Upload;
    using Models.Calculator;
    using System.Web.Mvc;
    using Models;
    using System.Configuration;
    using System.Data.SqlClient;
    using System.Data;

    public class CalculatorController : Controller
    {
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(UploadViewModel uploadViewModel)
        {
            var model = new CalculatorViewModel(uploadViewModel.EncodedSaveData);

            if (uploadViewModel.AddToProgress && this.Request.IsAuthenticated)
            {
                var user = this.User.Identity as ApplicationUser;
                var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand("UploadSaveData", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Upload data
                        command.Parameters.AddWithValue("@UserId", user.Id);
                        command.Parameters.AddWithValue("@UploadContent", uploadViewModel.EncodedSaveData);

                        // Computed stats
                        command.Parameters.AddWithValue("@OptimalLevel", model.ComputedStatsViewModel.OptimalLevel);
                        command.Parameters.AddWithValue("@SoulsPerHour", model.ComputedStatsViewModel.SoulsPerHour);
                        command.Parameters.AddWithValue("@SoulsPerAscension", model.ComputedStatsViewModel.OptimalSoulsPerAscension);
                        command.Parameters.AddWithValue("@AscensionTime", model.ComputedStatsViewModel.OptimalAscensionTime);

                        // Ancient levels
                        DataTable ancientLevelTable = new DataTable();
                        ancientLevelTable.Columns.Add("AncientId", typeof(int));
                        ancientLevelTable.Columns.Add("Level", typeof(int));
                        foreach (var pair in model.AncientLevelSummaryViewModel.AncientLevels)
                        {
                            ancientLevelTable.Rows.Add(pair.Key.Id, pair.Value);
                        }

                        var ancientLevelsParam = command.Parameters.AddWithValue("@AncientLevelUploads", ancientLevelTable);
                        ancientLevelsParam.SqlDbType = SqlDbType.Structured;
                        ancientLevelsParam.TypeName = "dbo.AncientLevelUpload";

                        command.ExecuteNonQuery();
                    }
                }
            }

            return this.View(model);
        }
    }
}