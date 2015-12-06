// <copyright file="UploadController.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Controllers
{
    using System.Web.Mvc;

    public class UploadController : Controller
    {
        // GET: Upload
        [HttpGet]
        public ActionResult Index()
        {
            return this.View();
        }
    }
}