namespace ClickerHeroesTrackerWebsite.Controllers
{
    using Models.Dashboard;
    using System.Web.Mvc;

    [Authorize]
    public class DashboardController : Controller
    {
        // GET: Dashboard
        public ActionResult Index()
        {
            var model = new DashboardViewModel(this.User);

            return View(model);
        }
    }
}