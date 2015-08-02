namespace ClickerHeroesTrackerWebsite.Controllers
{
    using ClickerHeroesTrackerWebsite.Models;
    using Models.Upload;
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using System.Text;
    using System.Web.Mvc;

    public class UploadController : Controller
    {
        // GET: Upload
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        // POST: Confirm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Confirm(UploadViewModel uploadViewModel)
        {
            var model = new ConfirmViewModel(uploadViewModel.EncodedSaveData);

            return this.View(model);
        }
    }
}