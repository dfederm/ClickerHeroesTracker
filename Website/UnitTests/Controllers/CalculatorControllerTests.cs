// <copyright file="CalculatorControllerTests.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Tests.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using ClickerHeroesTrackerWebsite.Controllers;
    using ClickerHeroesTrackerWebsite.Models.Calculator;
    using ClickerHeroesTrackerWebsite.Models.Settings;
    using ClickerHeroesTrackerWebsite.Services.Database;
    using ClickerHeroesTrackerWebsite.Tests.Mocks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using Website.Models.Api.Users;
    using Xunit;

    public class CalculatorControllerTests
    {
        [Fact]
        public void View_NoUploadId()
        {
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);

            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);

            var mockUserManager = MockUserManager.CreateMock();

            var controller = new CalculatorController(
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object);

            var result = controller.View(null);

            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);

            var viewResult = (ViewResult)result;
            Assert.Null(viewResult.Model);
            Assert.Equal("Error", viewResult.ViewName);
            Assert.Equal("The upload does not exist", viewResult.ViewData["ErrorMessage"]);

            mockDatabaseCommandFactory.Verify();
            mockUserSettingsProvider.Verify();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public void View_NotExistingUpload()
        {
            var user = new ClaimsPrincipal();

            var mockDataReader = MockDatabaseHelper.CreateMockDataReader();
            var mockDatabaseCommand = MockDatabaseHelper.CreateMockDatabaseCommand(new Dictionary<string, object> { { "@UploadId", 1 } }, mockDataReader.Object);

            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            mockDatabaseCommandFactory.Setup(_ => _.Create()).Returns(mockDatabaseCommand.Object).Verifiable();

            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);

            var mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.GetUserId(user))
                .Returns((string)null);

            var mockHttpContext = new Mock<HttpContext>(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(user).Verifiable();
            mockHttpContext.SetupGet(_ => _.RequestServices).Returns((IServiceProvider)null).Verifiable();

            var controller = new CalculatorController(
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            var result = controller.View(1);

            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);

            var viewResult = (ViewResult)result;

            Assert.Null(viewResult.Model);
            Assert.Equal("Error", viewResult.ViewName);
            Assert.Equal("The upload does not exist", viewResult.ViewData["ErrorMessage"]);

            mockDataReader.Verify();
            mockDatabaseCommand.Verify();
            mockDatabaseCommandFactory.Verify();
            mockUserSettingsProvider.Verify();
            mockHttpContext.Verify();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Theory]
        [InlineData(null, "User1", false, false)]
        [InlineData(null, "User1", true, true)]
        [InlineData("User2", null, false, true)]
        [InlineData("User2", null, true, true)]
        [InlineData("User2", "User1", false, false)]
        [InlineData("User1", "User1", true, true)]
        [InlineData("Admin", null, false, true)]
        [InlineData("Admin", null, true, true)]
        [InlineData("Admin", "User1", false, true)]
        [InlineData("Admin", "Admin", true, true)]
        public void View_ExistingUpload(string userId, string uploadUserId, bool isPublic, bool expectedPermitted)
        {
            var claims = new List<Claim>();
            if (userId != null)
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
                claims.Add(new Claim(ClaimTypes.Role, userId));
            }

            var user = new ClaimsPrincipal(new ClaimsIdentity(claims));

            var mockDataReader = MockDatabaseHelper.CreateMockDataReader(new Dictionary<string, object> { { "UserId", uploadUserId } });
            var mockDatabaseCommand = MockDatabaseHelper.CreateMockDatabaseCommand(new Dictionary<string, object> { { "@UploadId", 1 } }, mockDataReader.Object);

            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            mockDatabaseCommandFactory.Setup(_ => _.Create()).Returns(mockDatabaseCommand.Object).Verifiable();

            var userSettings = new UserSettings { AreUploadsPublic = isPublic };
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
            mockUserSettingsProvider.Setup(_ => _.Get(uploadUserId)).Returns(userSettings).Verifiable();

            var mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.GetUserId(user))
                .Returns(userId);

            var mockHttpContext = new Mock<HttpContext>(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(user).Verifiable();
            mockHttpContext.SetupGet(_ => _.RequestServices).Returns((IServiceProvider)null).Verifiable();

            var controller = new CalculatorController(
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            var result = controller.View(1);

            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);

            var viewResult = (ViewResult)result;

            if (expectedPermitted)
            {
                Assert.Equal("Calculator", viewResult.ViewName);
                Assert.NotNull(viewResult.Model);
                Assert.IsType<CalculatorViewModel>(viewResult.Model);

                var model = (CalculatorViewModel)viewResult.Model;
                Assert.Equal(1, model.UploadId);
                Assert.True(model.IsValid);
                Assert.True(model.IsPermitted);
                Assert.Equal(userId == uploadUserId, model.IsOwn);
                Assert.Equal(isPublic || uploadUserId == null, model.IsPublic);
                Assert.NotNull(model.SuggestedAncientIds);
                Assert.NotEmpty(model.SuggestedAncientIds);
            }
            else
            {
                Assert.Equal("Error", viewResult.ViewName);
                Assert.Null(viewResult.Model);
                Assert.Equal("This upload belongs to a user with private uploads", viewResult.ViewData["ErrorMessage"]);
            }

            mockDataReader.Verify();
            mockDatabaseCommand.Verify();
            mockDatabaseCommandFactory.Verify();
            mockUserSettingsProvider.Verify();
            mockHttpContext.Verify();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }
    }
}
