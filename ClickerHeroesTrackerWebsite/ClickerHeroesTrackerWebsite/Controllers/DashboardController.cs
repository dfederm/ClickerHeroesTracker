namespace ClickerHeroesTrackerWebsite.Controllers
{
    using Models.Dashboard;
    using System.Web.Mvc;
    using Database;
    using Models.Settings;

    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IDatabaseCommandFactory databaseCommandFactory;

        private readonly IUserSettingsProvider userSettingsProvider;

        public DashboardController(
            IDatabaseCommandFactory databaseCommandFactory,
            IUserSettingsProvider userSettingsProvider)
        {
            this.databaseCommandFactory = databaseCommandFactory;
            this.userSettingsProvider = userSettingsProvider;
        }

        public ActionResult Index()
        {
            var model = new DashboardViewModel(
                this.databaseCommandFactory,
                this.userSettingsProvider,
                this.User);
            if (!model.IsValid)
            {
                this.ViewBag.ErrorMessage = "You have no uploaded data!";
                return View("Error");
            }

            return View(model);
        }

        public ActionResult Uploads()
        {
            return View();
        }

        public ActionResult Progress()
        {
            var model = new ProgressViewModel(
                this.databaseCommandFactory,
                this.userSettingsProvider,
                this.User);
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

            var model = new RivalViewModel(
                this.databaseCommandFactory,
                this.userSettingsProvider,
                this.User,
                rivalId);
            if (!model.IsValid)
            {
                this.ViewBag.ErrorMessage = "There was a problem comparing your data to that rival. Man sure they're your rival and have upload data.";
                return View("Error");
            }

            return View(model);
        }
    }
}