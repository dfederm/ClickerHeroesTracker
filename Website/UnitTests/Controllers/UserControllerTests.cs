// <copyright file="UserControllerTests.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace UnitTests.Controllers
{
    using System.Threading.Tasks;
    using ClickerHeroesTrackerWebsite.Models;
    using ClickerHeroesTrackerWebsite.Models.Settings;
    using ClickerHeroesTrackerWebsite.Services.Database;
    using ClickerHeroesTrackerWebsite.Tests.Mocks;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using Website.Controllers.Api;
    using Website.Models.Api.Users;
    using Xunit;

    public sealed class UserControllerTests
    {
        [Fact]
        public async Task Create_Success()
        {
            var createUser = new CreateUserRequest
            {
                UserName = "SomeUserName",
                Email = "SomeEmail",
                Password = "SomePassword",
            };

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);

            var mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.CreateAsync(It.Is<ApplicationUser>(user => user.UserName == createUser.UserName && user.Email == createUser.Email), createUser.Password))
                .Returns(Task.FromResult(IdentityResult.Success));

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object);
            var result = await controller.Create(createUser);

            Assert.NotNull(result);
            Assert.IsType<OkResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public async Task Create_InvalidModelState()
        {
            var createUser = new CreateUserRequest();
            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
            var mockUserManager = MockUserManager.CreateMock();

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object);
            controller.ModelState.AddModelError("SomeKey", "SomeErrorMessage");
            var result = await controller.Create(createUser);

            Assert.NotNull(result);
            Assert.IsType<BadRequestObjectResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public async Task Create_UserCreationFails()
        {
            var createUser = new CreateUserRequest
            {
                UserName = "SomeUserName",
                Email = "SomeEmail",
                Password = "SomePassword",
            };

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);

            var mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.CreateAsync(It.Is<ApplicationUser>(user => user.UserName == createUser.UserName && user.Email == createUser.Email), createUser.Password))
                .Returns(Task.FromResult(IdentityResult.Failed(new IdentityError { Description = "SomeDescription" })));

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object);
            var result = await controller.Create(createUser);

            Assert.NotNull(result);
            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(1, controller.ModelState.ErrorCount);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }
    }
}