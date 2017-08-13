// <copyright file="ErrorControllerTests.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Tests.Controllers
{
    using ClickerHeroesTrackerWebsite.Controllers;
    using Microsoft.AspNetCore.Mvc;
    using Xunit;

    public class ErrorControllerTests
    {
        [Fact]
        public void Index()
        {
            var controller = new ErrorController();

            var result = controller.Index();

            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);

            var viewResult = (ViewResult)result;
            Assert.Null(viewResult.Model);
            Assert.Equal("Error", viewResult.ViewName);
        }
    }
}