// <copyright file="HomeControllerTests.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Tests.Controllers
{
    using ClickerHeroesTrackerWebsite.Controllers;
    using Microsoft.AspNetCore.Mvc;
    using Xunit;

    public class HomeControllerTests
    {
        [Fact]
        public void Index()
        {
            var controller = new HomeController();

            var result = controller.Index();

            Assert.IsType<ViewResult>(result);
            Assert.NotNull(result);
        }

        [Fact]
        public void New()
        {
            var controller = new HomeController();

            var result = controller.New();

            Assert.IsType<ViewResult>(result);
            Assert.NotNull(result);
        }
    }
}
