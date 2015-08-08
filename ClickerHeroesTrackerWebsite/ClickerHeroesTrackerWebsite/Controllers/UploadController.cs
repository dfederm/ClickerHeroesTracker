namespace ClickerHeroesTrackerWebsite.Controllers
{
    using System.Web.Mvc;

    public class UploadController : Controller
    {
        // GET: Upload
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
    }
}