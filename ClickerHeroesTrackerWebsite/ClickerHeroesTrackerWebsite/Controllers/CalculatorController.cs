namespace ClickerHeroesTrackerWebsite.Controllers
{
    using Models.Upload;
    using Models.Calculator;
    using System.Web.Mvc;
    using System.Data;
    using Microsoft.AspNet.Identity;
    using Models;

    [Authorize]
    public class CalculatorController : Controller
    {
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult Upload(UploadViewModel uploadViewModel)
        {
            if (!ModelState.IsValid)
            {
                return this.RedirectToAction("Index", "Upload");
            }

            var model = new CalculatorViewModel(uploadViewModel.EncodedSaveData);

            if (uploadViewModel.AddToProgress
                && model.IsValid
                && this.Request.IsAuthenticated)
            {
                var userId = this.User.Identity.GetUserId();
                using (var command = new DatabaseCommand("UploadSaveData"))
                {
                    // Upload data
                    command.AddParameter("@UserId", userId);
                    command.AddParameter("@UploadContent", uploadViewModel.EncodedSaveData);

                    // Computed stats
                    command.AddParameter("@OptimalLevel", model.ComputedStatsViewModel.OptimalLevel);
                    command.AddParameter("@SoulsPerHour", model.ComputedStatsViewModel.SoulsPerHour);
                    command.AddParameter("@SoulsPerAscension", model.ComputedStatsViewModel.OptimalSoulsPerAscension);
                    command.AddParameter("@AscensionTime", model.ComputedStatsViewModel.OptimalAscensionTime);

                    // Ancient levels
                    DataTable ancientLevelTable = new DataTable();
                    ancientLevelTable.Columns.Add("AncientId", typeof(int));
                    ancientLevelTable.Columns.Add("Level", typeof(int));
                    foreach (var pair in model.AncientLevelSummaryViewModel.AncientLevels)
                    {
                        ancientLevelTable.Rows.Add(pair.Key.Id, pair.Value);
                    }

                    command.AddTableParameter("@AncientLevelUploads", "AncientLevelUpload", ancientLevelTable);

                    command.ExecuteNonQuery();
                }
            }

            return this.GetResult(model, true);
        }

        public ActionResult View(int uploadId)
        {
            var userId = this.User.Identity.GetUserId();
            var model = new CalculatorViewModel(userId, uploadId);
            return this.GetResult(model, false);
        }

        private ActionResult GetResult(CalculatorViewModel model, bool wasDataPosted)
        {
            string errorMessage = null;
            if (!model.IsValid)
            {
                if (wasDataPosted)
                {
                    errorMessage = "The uploaded save was not valid";
                }
                else
                {
                    errorMessage = "The upload does not exist";
                }
            }

            if (!model.IsPermitted)
            {
                errorMessage = "You are not permitted to view others' uploads";
            }

            if (errorMessage != null)
            {
                this.ViewBag.ErrorMessage = errorMessage;
                return this.View("Error");
            }

            return this.View("Calculator", model);
        }
    }
}