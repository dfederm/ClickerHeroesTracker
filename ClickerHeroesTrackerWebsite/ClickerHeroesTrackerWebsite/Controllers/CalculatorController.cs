namespace ClickerHeroesTrackerWebsite.Controllers
{
    using Models.Upload;
    using Models.Calculator;
    using System.Web.Mvc;
    using Database;
    using Models.Settings;

    public class CalculatorController : Controller
    {
        private readonly IDatabaseCommandFactory databaseCommandFactory;

        private readonly IUserSettingsProvider userSettingsProvider;

        public CalculatorController(
            IDatabaseCommandFactory databaseCommandFactory,
            IUserSettingsProvider userSettingsProvider)
        {
            this.databaseCommandFactory = databaseCommandFactory;
            this.userSettingsProvider = userSettingsProvider;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Upload(UploadViewModel uploadViewModel)
        {
            if (!ModelState.IsValid)
            {
                return this.RedirectToAction("Index", "Upload");
            }

            var model = new CalculatorViewModel(
                this.databaseCommandFactory,
                this.userSettingsProvider,
                uploadViewModel.EncodedSaveData,
                this.User.Identity,
                uploadViewModel.AddToProgress);
            return this.GetResult(model, true);
        }

        public ActionResult View(int? uploadId)
        {
            CalculatorViewModel model = uploadId.HasValue
                ? new CalculatorViewModel(
                    this.databaseCommandFactory,
                    this.userSettingsProvider,
                    uploadId.Value,
                    this.User)
                : null;
            return this.GetResult(model, false);
        }

        private ActionResult GetResult(CalculatorViewModel model, bool wasDataPosted)
        {
            string errorMessage = null;
            if (model == null || !model.IsValid)
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
            else if (!model.IsPermitted)
            {
                errorMessage = "This upload belongs to a user with private uploads";
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