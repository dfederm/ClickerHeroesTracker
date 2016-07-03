// <copyright file="UploadController.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// The upload controller allows users to upload their saved games
    /// </summary>
    public class UploadController : Controller
    {
        /// <summary>
        /// GET: Upload
        /// </summary>
        /// <returns>The upload view</returns>
        [HttpGet]
        public ActionResult Index()
        {
            return this.View();
        }
    }
}