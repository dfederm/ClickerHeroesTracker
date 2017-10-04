// <copyright file="UserControllerTests.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace UnitTests.Controllers
{
    using System.Security.Claims;
    using System.Threading.Tasks;
    using ClickerHeroesTrackerWebsite.Models;
    using ClickerHeroesTrackerWebsite.Models.Settings;
    using ClickerHeroesTrackerWebsite.Services.Database;
    using ClickerHeroesTrackerWebsite.Tests.Mocks;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Http;
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

        [Fact]
        public async Task Settings_Success()
        {
            const string UserName = "SomeUserName";
            const string UserId = "SomeUserId";

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);

            var mockUserSettings = new Mock<IUserSettings>(MockBehavior.Strict);
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
            mockUserSettingsProvider
                .Setup(_ => _.Get(UserId))
                .Returns(mockUserSettings.Object);

            var mockUser = new ApplicationUser();
            var mockCurrentUser = new Mock<ClaimsPrincipal>(MockBehavior.Strict);
            var mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult(mockUser));
            mockUserManager
                .Setup(_ => _.GetUserIdAsync(mockUser))
                .Returns(Task.FromResult(UserId));
            mockUserManager
                .Setup(_ => _.GetUserId(mockCurrentUser.Object))
                .Returns(UserId);

            var mockHttpContext = new Mock<HttpContext>(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            var result = await controller.Settings(UserName);

            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mockUserSettings.Object, ((OkObjectResult)result).Value);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockUserSettings.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockHttpContext.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public async Task Settings_MissingUserName()
        {
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

            var result = await controller.Settings(null);

            Assert.NotNull(result);
            Assert.IsType<BadRequestResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public async Task Settings_NotFoundUser()
        {
            const string UserName = "SomeUserName";

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
            var mockUser = new ApplicationUser();
            var mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult<ApplicationUser>(null));

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object);

            var result = await controller.Settings(UserName);

            Assert.NotNull(result);
            Assert.IsType<NotFoundResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public async Task Settings_NotFoundUserId()
        {
            const string UserName = "SomeUserName";

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);

            var mockUser = new ApplicationUser();
            var mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult(mockUser));
            mockUserManager
                .Setup(_ => _.GetUserIdAsync(mockUser))
                .Returns(Task.FromResult<string>(null));

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object);

            var result = await controller.Settings(UserName);

            Assert.NotNull(result);
            Assert.IsType<NotFoundResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public async Task Settings_NotAllowed()
        {
            const string UserName = "SomeUserName";
            const string UserId = "SomeUserId";

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);

            var mockUser = new ApplicationUser();
            var mockCurrentUser = new Mock<ClaimsPrincipal>(MockBehavior.Strict);
            mockCurrentUser
                .Setup(_ => _.IsInRole("Admin"))
                .Returns(false);
            var mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult(mockUser));
            mockUserManager
                .Setup(_ => _.GetUserIdAsync(mockUser))
                .Returns(Task.FromResult(UserId));
            mockUserManager
                .Setup(_ => _.GetUserId(mockCurrentUser.Object))
                .Returns("SomeOtherUserId");

            var mockHttpContext = new Mock<HttpContext>(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            var result = await controller.Settings(UserName);

            Assert.NotNull(result);
            Assert.IsType<ForbidResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockHttpContext.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public async Task Settings_AdminIsAlwaysAllowed()
        {
            const string UserName = "SomeUserName";
            const string UserId = "SomeUserId";

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);

            var mockUserSettings = new Mock<IUserSettings>(MockBehavior.Strict);
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
            mockUserSettingsProvider
                .Setup(_ => _.Get(UserId))
                .Returns(mockUserSettings.Object);

            var mockUser = new ApplicationUser();
            var mockCurrentUser = new Mock<ClaimsPrincipal>(MockBehavior.Strict);
            mockCurrentUser
                .Setup(_ => _.IsInRole("Admin"))
                .Returns(true);
            var mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult(mockUser));
            mockUserManager
                .Setup(_ => _.GetUserIdAsync(mockUser))
                .Returns(Task.FromResult(UserId));
            mockUserManager
                .Setup(_ => _.GetUserId(mockCurrentUser.Object))
                .Returns("SomeOtherUserId");

            var mockHttpContext = new Mock<HttpContext>(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            var result = await controller.Settings(UserName);

            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mockUserSettings.Object, ((OkObjectResult)result).Value);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockUserSettings.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockHttpContext.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }
    }
}