namespace ClickerHeroesTrackerWebsite.Controllers
{
    using Models.Home;
    using System.Web.Mvc;

    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult New()
        {
            return View();
        }
    }
}