// <copyright file="HomeController.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Controllers
{
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    // Add the Mock AuthenticationScheme for Integration tests.
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme + ",Mock")]
    [AllowAnonymous]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return this.View();
        }

        public ActionResult New()
        {
            return this.View();
        }

        public ActionResult Beta()
        {
            return this.View();
        }
    }
}