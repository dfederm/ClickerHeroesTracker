namespace ClickerHeroesTrackerWebsite.Controllers
{
    using Models.Upload;
    using Models.Calculator;
    using System.Web.Mvc;
    using Microsoft.AspNet.Identity;

    public class CalculatorController : Controller
    {
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Upload(UploadViewModel uploadViewModel)
        {
            if (!ModelState.IsValid)
            {
                return this.RedirectToAction("Index", "Upload");
            }

            var model = new CalculatorViewModel(uploadViewModel.EncodedSaveData, this.User.Identity, uploadViewModel.AddToProgress);
            return this.GetResult(model, true);
        }

        public ActionResult View(int uploadId)
        {
            var userId = this.User.Identity.GetUserId();
            var model = new CalculatorViewModel(uploadId, userId);
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