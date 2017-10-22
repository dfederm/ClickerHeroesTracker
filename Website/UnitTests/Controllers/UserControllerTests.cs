// <copyright file="UserControllerTests.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace UnitTests.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using ClickerHeroesTrackerWebsite.Models;
    using ClickerHeroesTrackerWebsite.Models.Settings;
    using ClickerHeroesTrackerWebsite.Services.Database;
    using ClickerHeroesTrackerWebsite.Services.Email;
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
            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object);
            var result = await controller.Create(createUser);

            Assert.NotNull(result);
            Assert.IsType<OkResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockEmailSender.VerifyAll();

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
            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object);
            controller.ModelState.AddModelError("SomeKey", "SomeErrorMessage");
            var result = await controller.Create(createUser);

            Assert.NotNull(result);
            Assert.IsType<BadRequestObjectResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockEmailSender.VerifyAll();

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
            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object);
            var result = await controller.Create(createUser);

            Assert.NotNull(result);
            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(1, controller.ModelState.ErrorCount);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockEmailSender.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public async Task GetSettings_Success()
        {
            const string UserName = "SomeUserName";
            const string UserId = "SomeUserId";

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);

            var userSettings = new UserSettings();
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
            mockUserSettingsProvider
                .Setup(_ => _.Get(UserId))
                .Returns(userSettings);

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

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            var result = await controller.GetSettings(UserName);

            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(userSettings, ((OkObjectResult)result).Value);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockHttpContext.VerifyAll();
            mockEmailSender.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public async Task GetSettings_MissingUserName()
        {
            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
            var mockUserManager = MockUserManager.CreateMock();
            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object);

            var result = await controller.GetSettings(null);

            Assert.NotNull(result);
            Assert.IsType<BadRequestResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockEmailSender.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public async Task GetSettings_NotFoundUser()
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

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object);

            var result = await controller.GetSettings(UserName);

            Assert.NotNull(result);
            Assert.IsType<NotFoundResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockEmailSender.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public async Task GetSettings_NotFoundUserId()
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

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object);

            var result = await controller.GetSettings(UserName);

            Assert.NotNull(result);
            Assert.IsType<NotFoundResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockEmailSender.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public async Task GetSettings_NotAllowed()
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

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            var result = await controller.GetSettings(UserName);

            Assert.NotNull(result);
            Assert.IsType<ForbidResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockHttpContext.VerifyAll();
            mockEmailSender.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public async Task GetSettings_AdminIsAlwaysAllowed()
        {
            const string UserName = "SomeUserName";
            const string UserId = "SomeUserId";

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);

            var userSettings = new UserSettings();
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
            mockUserSettingsProvider
                .Setup(_ => _.Get(UserId))
                .Returns(userSettings);

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

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            var result = await controller.GetSettings(UserName);

            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(userSettings, ((OkObjectResult)result).Value);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockHttpContext.VerifyAll();
            mockEmailSender.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public async Task PatchSettings_Success()
        {
            const string UserName = "SomeUserName";
            const string UserId = "SomeUserId";

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var userSettings = new UserSettings();
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
            mockUserSettingsProvider.Setup(_ => _.Patch(UserId, userSettings));

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

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            var result = await controller.PatchSettings(UserName, userSettings);

            Assert.NotNull(result);
            Assert.IsType<OkResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockHttpContext.VerifyAll();
            mockEmailSender.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public async Task PatchSettings_MissingUserName()
        {
            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var userSettings = new UserSettings();
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
            var mockUserManager = MockUserManager.CreateMock();
            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object);

            var result = await controller.PatchSettings(null, userSettings);

            Assert.NotNull(result);
            Assert.IsType<BadRequestResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockEmailSender.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public async Task PatchSettings_MissingUserSettings()
        {
            const string UserName = "SomeUserName";

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
            var mockUserManager = MockUserManager.CreateMock();
            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object);

            var result = await controller.PatchSettings(UserName, null);

            Assert.NotNull(result);
            Assert.IsType<BadRequestResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockEmailSender.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public async Task PatchSettings_NotFoundUser()
        {
            const string UserName = "SomeUserName";

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var userSettings = new UserSettings();
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
            var mockUser = new ApplicationUser();
            var mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult<ApplicationUser>(null));

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object);

            var result = await controller.PatchSettings(UserName, userSettings);

            Assert.NotNull(result);
            Assert.IsType<NotFoundResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockEmailSender.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public async Task PatchSettings_NotFoundUserId()
        {
            const string UserName = "SomeUserName";

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var userSettings = new UserSettings();
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);

            var mockUser = new ApplicationUser();
            var mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult(mockUser));
            mockUserManager
                .Setup(_ => _.GetUserIdAsync(mockUser))
                .Returns(Task.FromResult<string>(null));

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object);

            var result = await controller.PatchSettings(UserName, userSettings);

            Assert.NotNull(result);
            Assert.IsType<NotFoundResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockEmailSender.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public async Task PatchSettings_NotAllowed()
        {
            const string UserName = "SomeUserName";
            const string UserId = "SomeUserId";

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var userSettings = new UserSettings();
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

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            var result = await controller.PatchSettings(UserName, userSettings);

            Assert.NotNull(result);
            Assert.IsType<ForbidResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockHttpContext.VerifyAll();
            mockEmailSender.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public async Task GetLogins_Success()
        {
            const string UserName = "SomeUserName";
            const string UserId = "SomeUserId";

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
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
            mockUserManager
                .Setup(_ => _.GetUserId(mockCurrentUser.Object))
                .Returns(UserId);
            mockUserManager
                .Setup(_ => _.HasPasswordAsync(mockUser))
                .Returns(Task.FromResult(true));

            var logins = new[] { "SomeLogin0", "SomeLogin1", "SomeLogin2" };
            mockUserManager
                .Setup(_ => _.GetLoginsAsync(mockUser))
                .Returns(Task.FromResult<IList<UserLoginInfo>>(logins.Select(login => new UserLoginInfo(login, "SomeProviderKey", "SomeDisplayName")).ToList()));

            var mockHttpContext = new Mock<HttpContext>(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            var result = await controller.GetLogins(UserName);

            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);

            var model = ((OkObjectResult)result).Value as UserLogins;
            Assert.True(model.HasPassword);
            Assert.Equal(logins, model.ExternalLogins);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockHttpContext.VerifyAll();
            mockEmailSender.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public async Task GetLogins_MissingUserName()
        {
            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
            var mockUserManager = MockUserManager.CreateMock();
            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object);

            var result = await controller.GetLogins(null);

            Assert.NotNull(result);
            Assert.IsType<BadRequestResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockEmailSender.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public async Task GetLogins_NotFoundUser()
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

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object);

            var result = await controller.GetLogins(UserName);

            Assert.NotNull(result);
            Assert.IsType<NotFoundResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockEmailSender.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public async Task GetLogins_NotFoundUserId()
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

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object);

            var result = await controller.GetLogins(UserName);

            Assert.NotNull(result);
            Assert.IsType<NotFoundResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockEmailSender.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public async Task GetLogins_NotAllowed()
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

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            var result = await controller.GetLogins(UserName);

            Assert.NotNull(result);
            Assert.IsType<ForbidResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockHttpContext.VerifyAll();
            mockEmailSender.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public async Task GetLogins_AdminIsAlwaysAllowed()
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
            mockUserManager
                .Setup(_ => _.HasPasswordAsync(mockUser))
                .Returns(Task.FromResult(true));

            var logins = new[] { "SomeLogin0", "SomeLogin1", "SomeLogin2" };
            mockUserManager
                .Setup(_ => _.GetLoginsAsync(mockUser))
                .Returns(Task.FromResult<IList<UserLoginInfo>>(logins.Select(login => new UserLoginInfo(login, "SomeProviderKey", "SomeDisplayName")).ToList()));

            var mockHttpContext = new Mock<HttpContext>(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            var result = await controller.GetLogins(UserName);

            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);

            var model = ((OkObjectResult)result).Value as UserLogins;
            Assert.True(model.HasPassword);
            Assert.Equal(logins, model.ExternalLogins);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockHttpContext.VerifyAll();
            mockEmailSender.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public async Task PatchSettings_AdminIsAlwaysAllowed()
        {
            const string UserName = "SomeUserName";
            const string UserId = "SomeUserId";

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var userSettings = new UserSettings();
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
            mockUserSettingsProvider.Setup(_ => _.Patch(UserId, userSettings));

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

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            var result = await controller.PatchSettings(UserName, userSettings);

            Assert.NotNull(result);
            Assert.IsType<OkResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockHttpContext.VerifyAll();
            mockEmailSender.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public async Task SetPassword_Success()
        {
            const string UserName = "SomeUserName";
            const string UserId = "SomeUserId";
            const string NewPassword = "SomeNewPassword";

            var model = new SetPasswordRequest
            {
                NewPassword = NewPassword,
            };

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
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
            mockUserManager
                .Setup(_ => _.AddPasswordAsync(mockUser, NewPassword))
                .Returns(Task.FromResult(IdentityResult.Success));

            var mockHttpContext = new Mock<HttpContext>(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            var result = await controller.SetPassword(UserName, model);

            Assert.NotNull(result);
            Assert.IsType<OkResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockHttpContext.VerifyAll();
            mockEmailSender.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public async Task SetPassword_MissingUserName()
        {
            const string NewPassword = "SomeNewPassword";

            var model = new SetPasswordRequest
            {
                NewPassword = NewPassword,
            };

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
            var mockUserManager = MockUserManager.CreateMock();
            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object);

            var result = await controller.SetPassword(null, model);

            Assert.NotNull(result);
            Assert.IsType<BadRequestResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockEmailSender.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public async Task SetPassword_InvalidModelState()
        {
            const string UserName = "SomeUserName";

            var model = new SetPasswordRequest();
            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
            var mockUserManager = MockUserManager.CreateMock();
            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object);
            controller.ModelState.AddModelError("SomeKey", "SomeErrorMessage");
            var result = await controller.SetPassword(UserName, model);

            Assert.NotNull(result);
            Assert.IsType<BadRequestObjectResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public async Task SetPassword_NotFoundUser()
        {
            const string UserName = "SomeUserName";
            const string NewPassword = "SomeNewPassword";

            var model = new SetPasswordRequest
            {
                NewPassword = NewPassword,
            };

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
            var mockUser = new ApplicationUser();
            var mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult<ApplicationUser>(null));

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object);

            var result = await controller.SetPassword(UserName, model);

            Assert.NotNull(result);
            Assert.IsType<NotFoundResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockEmailSender.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public async Task SetPassword_NotFoundUserId()
        {
            const string UserName = "SomeUserName";
            const string NewPassword = "SomeNewPassword";

            var model = new SetPasswordRequest
            {
                NewPassword = NewPassword,
            };

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

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object);

            var result = await controller.SetPassword(UserName, model);

            Assert.NotNull(result);
            Assert.IsType<NotFoundResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockEmailSender.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public async Task SetPassword_NotAllowed()
        {
            const string UserName = "SomeUserName";
            const string UserId = "SomeUserId";
            const string NewPassword = "SomeNewPassword";

            var model = new SetPasswordRequest
            {
                NewPassword = NewPassword,
            };

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

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            var result = await controller.SetPassword(UserName, model);

            Assert.NotNull(result);
            Assert.IsType<ForbidResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockHttpContext.VerifyAll();
            mockEmailSender.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public async Task SetPassword_AdminIsAlwaysAllowed()
        {
            const string UserName = "SomeUserName";
            const string UserId = "SomeUserId";
            const string NewPassword = "SomeNewPassword";

            var model = new SetPasswordRequest
            {
                NewPassword = NewPassword,
            };

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);

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
            mockUserManager
                .Setup(_ => _.AddPasswordAsync(mockUser, NewPassword))
                .Returns(Task.FromResult(IdentityResult.Success));

            var mockHttpContext = new Mock<HttpContext>(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            var result = await controller.SetPassword(UserName, model);

            Assert.NotNull(result);
            Assert.IsType<OkResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockHttpContext.VerifyAll();
            mockEmailSender.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public async Task SetPassword_SetPasswordFails()
        {
            const string UserName = "SomeUserName";
            const string UserId = "SomeUserId";
            const string NewPassword = "SomeNewPassword";

            var model = new SetPasswordRequest
            {
                NewPassword = NewPassword,
            };

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
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
            mockUserManager
                .Setup(_ => _.AddPasswordAsync(mockUser, NewPassword))
                .Returns(Task.FromResult(IdentityResult.Failed(new IdentityError { Description = "SomeDescription" })));

            var mockHttpContext = new Mock<HttpContext>(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            var result = await controller.SetPassword(UserName, model);

            Assert.NotNull(result);
            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(1, controller.ModelState.ErrorCount);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockHttpContext.VerifyAll();
            mockEmailSender.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public async Task ChangePassword_Success()
        {
            const string UserName = "SomeUserName";
            const string UserId = "SomeUserId";
            const string CurrentPassword = "SomeCurrentPassword";
            const string NewPassword = "SomeNewPassword";

            var model = new ChangePasswordRequest
            {
                CurrentPassword = CurrentPassword,
                NewPassword = NewPassword,
            };

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
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
            mockUserManager
                .Setup(_ => _.ChangePasswordAsync(mockUser, CurrentPassword, NewPassword))
                .Returns(Task.FromResult(IdentityResult.Success));

            var mockHttpContext = new Mock<HttpContext>(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            var result = await controller.ChangePassword(UserName, model);

            Assert.NotNull(result);
            Assert.IsType<OkResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockHttpContext.VerifyAll();
            mockEmailSender.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public async Task ChangePassword_MissingUserName()
        {
            const string CurrentPassword = "SomeCurrentPassword";
            const string NewPassword = "SomeNewPassword";

            var model = new ChangePasswordRequest
            {
                CurrentPassword = CurrentPassword,
                NewPassword = NewPassword,
            };

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
            var mockUserManager = MockUserManager.CreateMock();
            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object);

            var result = await controller.ChangePassword(null, model);

            Assert.NotNull(result);
            Assert.IsType<BadRequestResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockEmailSender.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public async Task ChangePassword_InvalidModelState()
        {
            const string UserName = "SomeUserName";

            var model = new ChangePasswordRequest();
            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
            var mockUserManager = MockUserManager.CreateMock();
            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object);
            controller.ModelState.AddModelError("SomeKey", "SomeErrorMessage");
            var result = await controller.ChangePassword(UserName, model);

            Assert.NotNull(result);
            Assert.IsType<BadRequestObjectResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public async Task ChangePassword_NotFoundUser()
        {
            const string UserName = "SomeUserName";
            const string CurrentPassword = "SomeCurrentPassword";
            const string NewPassword = "SomeNewPassword";

            var model = new ChangePasswordRequest
            {
                CurrentPassword = CurrentPassword,
                NewPassword = NewPassword,
            };

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
            var mockUser = new ApplicationUser();
            var mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult<ApplicationUser>(null));

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object);

            var result = await controller.ChangePassword(UserName, model);

            Assert.NotNull(result);
            Assert.IsType<NotFoundResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockEmailSender.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public async Task ChangePassword_NotFoundUserId()
        {
            const string UserName = "SomeUserName";
            const string CurrentPassword = "SomeCurrentPassword";
            const string NewPassword = "SomeNewPassword";

            var model = new ChangePasswordRequest
            {
                CurrentPassword = CurrentPassword,
                NewPassword = NewPassword,
            };

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

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object);

            var result = await controller.ChangePassword(UserName, model);

            Assert.NotNull(result);
            Assert.IsType<NotFoundResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockEmailSender.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public async Task ChangePassword_NotAllowed()
        {
            const string UserName = "SomeUserName";
            const string UserId = "SomeUserId";
            const string CurrentPassword = "SomeCurrentPassword";
            const string NewPassword = "SomeNewPassword";

            var model = new ChangePasswordRequest
            {
                CurrentPassword = CurrentPassword,
                NewPassword = NewPassword,
            };

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

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            var result = await controller.ChangePassword(UserName, model);

            Assert.NotNull(result);
            Assert.IsType<ForbidResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockHttpContext.VerifyAll();
            mockEmailSender.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public async Task ChangePassword_AdminIsAlwaysAllowed()
        {
            const string UserName = "SomeUserName";
            const string UserId = "SomeUserId";
            const string CurrentPassword = "SomeCurrentPassword";
            const string NewPassword = "SomeNewPassword";

            var model = new ChangePasswordRequest
            {
                CurrentPassword = CurrentPassword,
                NewPassword = NewPassword,
            };

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);

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
            mockUserManager
                .Setup(_ => _.ChangePasswordAsync(mockUser, CurrentPassword, NewPassword))
                .Returns(Task.FromResult(IdentityResult.Success));

            var mockHttpContext = new Mock<HttpContext>(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            var result = await controller.ChangePassword(UserName, model);

            Assert.NotNull(result);
            Assert.IsType<OkResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockHttpContext.VerifyAll();
            mockEmailSender.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public async Task ChangePassword_ChangePasswordFails()
        {
            const string UserName = "SomeUserName";
            const string UserId = "SomeUserId";
            const string CurrentPassword = "SomeCurrentPassword";
            const string NewPassword = "SomeNewPassword";

            var model = new ChangePasswordRequest
            {
                CurrentPassword = CurrentPassword,
                NewPassword = NewPassword,
            };

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
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
            mockUserManager
                .Setup(_ => _.ChangePasswordAsync(mockUser, CurrentPassword, NewPassword))
                .Returns(Task.FromResult(IdentityResult.Failed(new IdentityError { Description = "SomeDescription" })));

            var mockHttpContext = new Mock<HttpContext>(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            var result = await controller.ChangePassword(UserName, model);

            Assert.NotNull(result);
            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(1, controller.ModelState.ErrorCount);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockHttpContext.VerifyAll();
            mockEmailSender.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public async Task ResetPassword_Success()
        {
            const string Code = "SomeCode";

            var model = new ResetPasswordRequest
            {
                Email = "SomeEmail",
            };

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);

            var mockUser = new ApplicationUser();
            var mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByEmailAsync(model.Email))
                .Returns(Task.FromResult(mockUser));
            mockUserManager
                .Setup(_ => _.GeneratePasswordResetTokenAsync(mockUser))
                .Returns(Task.FromResult(Code));

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);
            mockEmailSender
                .Setup(_ => _.SendEmailAsync(model.Email, It.IsAny<string>(), It.Is<string>(str => str.Contains(Code))))
                .Returns(Task.CompletedTask);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object);
            var result = await controller.ResetPassword(model);

            Assert.NotNull(result);
            Assert.IsType<OkResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockEmailSender.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public async Task ResetPassword_UserLookupFails()
        {
            var model = new ResetPasswordRequest
            {
                Email = "SomeEmail",
            };

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);

            var mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByEmailAsync(model.Email))
                .Returns(Task.FromResult((ApplicationUser)null));

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object);
            var result = await controller.ResetPassword(model);

            Assert.NotNull(result);
            Assert.IsType<OkResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public async Task ResetPassword_InvalidModelState()
        {
            var model = new ResetPasswordRequest();
            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
            var mockUserManager = MockUserManager.CreateMock();
            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object);
            controller.ModelState.AddModelError("SomeKey", "SomeErrorMessage");
            var result = await controller.ResetPassword(model);

            Assert.NotNull(result);
            Assert.IsType<BadRequestObjectResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public async Task ResetPasswordConfirmation_Success()
        {
            var model = new ResetPasswordConfirmationRequest
            {
                Email = "SomeEmail",
                Password = "SomePassword",
                Code = "SomeCode",
            };

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);

            var mockUser = new ApplicationUser();
            var mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByEmailAsync(model.Email))
                .Returns(Task.FromResult(mockUser));
            mockUserManager
                .Setup(_ => _.ResetPasswordAsync(mockUser, model.Code, model.Password))
                .Returns(Task.FromResult(IdentityResult.Success));

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object);
            var result = await controller.ResetPasswordConfirmation(model);

            Assert.NotNull(result);
            Assert.IsType<OkResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockEmailSender.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public async Task ResetPasswordConfirmation_UserLookupFails()
        {
            var model = new ResetPasswordConfirmationRequest
            {
                Email = "SomeEmail",
                Password = "SomePassword",
                Code = "SomeCode",
            };

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);

            var mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByEmailAsync(model.Email))
                .Returns(Task.FromResult((ApplicationUser)null));

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object);
            var result = await controller.ResetPasswordConfirmation(model);

            Assert.NotNull(result);
            Assert.IsType<OkResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public async Task ResetPasswordConfirmation_ResetPasswordFails()
        {
            var model = new ResetPasswordConfirmationRequest
            {
                Email = "SomeEmail",
                Password = "SomePassword",
                Code = "SomeCode",
            };

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);

            var mockUser = new ApplicationUser();
            var mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByEmailAsync(model.Email))
                .Returns(Task.FromResult(mockUser));
            mockUserManager
                .Setup(_ => _.ResetPasswordAsync(mockUser, model.Code, model.Password))
                .Returns(Task.FromResult(IdentityResult.Failed(new IdentityError { Description = "SomeDescription" })));

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object);
            var result = await controller.ResetPasswordConfirmation(model);

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
        public async Task ResetPasswordConfirmation_InvalidModelState()
        {
            var model = new ResetPasswordConfirmationRequest();
            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
            var mockUserManager = MockUserManager.CreateMock();
            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object);
            controller.ModelState.AddModelError("SomeKey", "SomeErrorMessage");
            var result = await controller.ResetPasswordConfirmation(model);

            Assert.NotNull(result);
            Assert.IsType<BadRequestObjectResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }
    }
}