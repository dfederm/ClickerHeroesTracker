namespace ClickerHeroesTrackerWebsite.Controllers
{
    using Models;
    using Microsoft.AspNet.Identity;
    using Models.Dashboard;
    using System.Web.Mvc;
    using System;

    [Authorize]
    public class DashboardController : Controller
    {
        public ActionResult Index()
        {
            var model = new DashboardViewModel(this.User);
            if (!model.IsValid)
            {
                this.ViewBag.ErrorMessage = "You have no uploaded data!";
                return View("Error");
            }

            return View(model);
        }

        public ActionResult Uploads(int? page)
        {
            var userId = this.User.Identity.GetUserId();

            var userSettings = new UserSettings(userId);
            userSettings.Fill();

            var model = new UploadDataSummary(
                userId,
                userSettings,
                page.GetValueOrDefault(),
                20);

            return View(model);
        }

        public ActionResult Progress()
        {
            var model = new ProgressViewModel(this.User);
            if (!model.IsValid)
            {
                this.ViewBag.ErrorMessage = "You have no uploaded data!";
                return View("Error");
            }

            return View(model);
        }

        public ActionResult Rival()
        {
            var rivalIdRaw = this.Request.QueryString["rivalId"];
            int rivalId;
            if (rivalIdRaw == null || !int.TryParse(rivalIdRaw, out rivalId))
            {
                return this.RedirectToAction("Index");
            }

            var model = new RivalViewModel(this.User, rivalId);
            if (!model.IsValid)
            {
                this.ViewBag.ErrorMessage = "There was a problem comparing your data to that rival. Man sure they're your rival and have upload data.";
                return View("Error");
            }

            return View(model);
        }
    }
}