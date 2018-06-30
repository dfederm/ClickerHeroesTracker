// <copyright file="UserControllerTests.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace UnitTests.Controllers
{
    using System;
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
    using Website.Controllers;
    using Website.Models.Api.Users;
    using Website.Services.Clans;
    using Xunit;

    public static class UserControllerTests
    {
        [Fact]
        public static async Task Create_Success()
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
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            var result = await controller.Create(createUser);

            Assert.NotNull(result);
            Assert.IsType<OkResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task Create_UserCreationFails()
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
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            var result = await controller.Create(createUser);

            Assert.NotNull(result);
            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(1, controller.ModelState.ErrorCount);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task Get_Success()
        {
            const string UserName = "SomeUserName";
            const string UserId = "SomeUserId";
            const string ClanName = "SomeClanName";

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);

            var mockUser = new ApplicationUser { UserName = UserName };
            var mockCurrentUser = new Mock<ClaimsPrincipal>(MockBehavior.Strict);

            var mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult(mockUser));
            mockUserManager
                .Setup(_ => _.GetUserIdAsync(mockUser))
                .Returns(Task.FromResult(UserId));

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);

            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);
            mockClanManager
                .Setup(_ => _.GetClanNameAsync(UserId))
                .Returns(Task.FromResult(ClanName));

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);

            var result = await controller.Get(UserName);
            Assert.NotNull(result);

            var model = result.Value;
            Assert.NotNull(model);

            Assert.Equal(UserName, model.Name);
            Assert.Equal(ClanName, model.ClanName);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task Get_NotFoundUser()
        {
            const string UserName = "SomeUserName";

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
            var mockCurrentUser = new Mock<ClaimsPrincipal>(MockBehavior.Strict);

            var mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult<ApplicationUser>(null));

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);

            var result = await controller.Get(UserName);

            Assert.NotNull(result);
            Assert.IsType<NotFoundResult>(result.Result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task Get_NotFoundUserId()
        {
            const string UserName = "SomeUserName";

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);

            var mockUser = new ApplicationUser { UserName = UserName };
            var mockCurrentUser = new Mock<ClaimsPrincipal>(MockBehavior.Strict);

            var mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult(mockUser));
            mockUserManager
                .Setup(_ => _.GetUserIdAsync(mockUser))
                .Returns(Task.FromResult<string>(null));

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);

            var result = await controller.Get(UserName);

            Assert.NotNull(result);
            Assert.IsType<NotFoundResult>(result.Result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task Uploads_Success()
        {
            const string UserName = "SomeUserName";
            const string UserId = "SomeUserId";
            const int Page = 12;
            const int Count = 34;
            const int TotalUploads = 1234;
            const string RequestPath = "/SomeRequestPath";

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();

            var expectedUploadIds = Enumerable.Range(0, 3).ToList();
            var getUploadsDatasets = expectedUploadIds
                .Select<int, IDictionary<string, object>>(id => new Dictionary<string, object>
                {
                    { "Id", id },
                    { "UploadTime", "2017-01-01T00:00:00.000Z" },
                    { "SaveTime", "2017-01-01T00:00:00.000Z" },
                    { "AscensionNumber", 100 },
                    { "Zone", 200 },
                    { "Souls", "1e300" },
                })
                .ToList();
            var mockGetUploadsDataReader = MockDatabaseHelper.CreateMockDataReader(getUploadsDatasets);
            var mockGetUploadsDatabaseCommandParameters = new Dictionary<string, object>() { { "@UserId", UserId }, { "@Offset", (Page - 1) * Count }, { "@Count", Count } };
            var mockGetUploadsDatabaseCommand = MockDatabaseHelper.CreateMockDatabaseCommand(mockGetUploadsDatabaseCommandParameters, mockGetUploadsDataReader.Object);

            var paginationDataset = new Dictionary<string, object> { { "TotalUploads", TotalUploads } };
            var mockPaginationDataReader = MockDatabaseHelper.CreateMockDataReader(paginationDataset);
            var mockPaginationDatabaseCommandParameters = new Dictionary<string, object>() { { "@UserId", UserId } };
            var mockPaginationDatabaseCommand = MockDatabaseHelper.CreateMockDatabaseCommand(mockPaginationDatabaseCommandParameters, mockPaginationDataReader.Object);

            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var commandCreationCount = 0;
            mockDatabaseCommandFactory
                .Setup(_ => _.Create())
                .Returns(() =>
                {
                    switch (commandCreationCount++)
                    {
                        case 0:
                            return mockGetUploadsDatabaseCommand.Object;
                        case 1:
                            return mockPaginationDatabaseCommand.Object;
                        default:
                            throw new InvalidOperationException("Unexpected call to DatabaseCommandFactory.Create");
                    }
                });

            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
            var mockUser = new ApplicationUser();
            var mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult(mockUser));
            mockUserManager
                .Setup(_ => _.GetUserIdAsync(mockUser))
                .Returns(Task.FromResult(UserId));

            // Use loose behavior since this is accessed a bunch internally in a controller
            var mockHttpRequest = new Mock<HttpRequest>(MockBehavior.Loose);
            mockHttpRequest.SetupGet(_ => _.Path).Returns(RequestPath);

            var mockHttpContext = new Mock<HttpContext>(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.Request).Returns(mockHttpRequest.Object);

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            var result = await controller.Uploads(UserName, Page, Count);
            Assert.NotNull(result);

            var model = result.Value;
            Assert.NotNull(model);

            Assert.NotNull(model.Uploads);
            Assert.Equal(expectedUploadIds.Count, model.Uploads.Count);
            for (var i = 0; i < model.Uploads.Count; i++)
            {
                Assert.Equal(expectedUploadIds[i], model.Uploads[i].Id);
            }

            Assert.NotNull(model.Pagination);
            Assert.Equal(TotalUploads, model.Pagination.Count);
            Assert.Equal($"{RequestPath}?page={Page - 1}&count={Count}", model.Pagination.Previous);
            Assert.Equal($"{RequestPath}?page={Page + 1}&count={Count}", model.Pagination.Next);

            mockGetUploadsDataReader.VerifyAll();
            mockGetUploadsDatabaseCommand.VerifyAll();
            mockPaginationDataReader.VerifyAll();
            mockPaginationDatabaseCommand.VerifyAll();
            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockHttpRequest.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Theory]
        [InlineData(
            "SomeUserName",
            UserController.ParameterConstants.Uploads.Page.Min - 1,
            UserController.ParameterConstants.Uploads.Count.Default)]
        [InlineData(
            "SomeUserName",
            UserController.ParameterConstants.Uploads.Page.Default,
            UserController.ParameterConstants.Uploads.Count.Min - 1)]
        [InlineData(
            "SomeUserName",
            UserController.ParameterConstants.Uploads.Page.Default,
            UserController.ParameterConstants.Uploads.Count.Max + 1)]
        public static async Task Uploads_ParameterValidation(string userName, int page, int count)
        {
            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
            var mockUserManager = MockUserManager.CreateMock();
            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);

            var result = await controller.Uploads(userName, page, count);

            Assert.NotNull(result);
            Assert.IsType<BadRequestResult>(result.Result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task Uploads_NotFoundUser()
        {
            const string UserName = "SomeUserName";
            const int Page = 12;
            const int Count = 34;

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);

            var mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult<ApplicationUser>(null));

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);

            var result = await controller.Uploads(UserName, Page, Count);

            Assert.NotNull(result);
            Assert.IsType<NotFoundResult>(result.Result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task Uploads_NotFoundUserId()
        {
            const string UserName = "SomeUserName";
            const int Page = 12;
            const int Count = 34;

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
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            var result = await controller.Uploads(UserName, Page, Count);

            Assert.NotNull(result);
            Assert.IsType<NotFoundResult>(result.Result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task Follows_Success()
        {
            const string UserName = "SomeUserName";
            const string UserId = "SomeUserId";

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();

            var follows = Enumerable.Range(0, 3)
                .Select(i => "SomeUser" + i)
                .ToList();
            var datasets = follows
                .Select<string, IDictionary<string, object>>(follow => new Dictionary<string, object> { { "UserName", follow } })
                .ToList();
            var mockDataReader = MockDatabaseHelper.CreateMockDataReader(datasets);
            var mockDatabaseCommandParameters = new Dictionary<string, object>() { { "@UserId", UserId } };
            var mockDatabaseCommand = MockDatabaseHelper.CreateMockDatabaseCommand(mockDatabaseCommandParameters, mockDataReader.Object);

            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            mockDatabaseCommandFactory
                .Setup(_ => _.Create())
                .Returns(mockDatabaseCommand.Object);

            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
            var mockUser = new ApplicationUser();
            var mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult(mockUser));
            mockUserManager
                .Setup(_ => _.GetUserIdAsync(mockUser))
                .Returns(Task.FromResult(UserId));

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);

            var result = await controller.Follows(UserName);
            Assert.NotNull(result);

            var model = result.Value;
            Assert.NotNull(model);
            Assert.Equal(follows.Count, model.Follows.Count);
            for (var i = 0; i < model.Follows.Count; i++)
            {
                Assert.Equal(follows[i], model.Follows[i]);
            }

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task Follows_NotFoundUser()
        {
            const string UserName = "SomeUserName";

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
            var mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult<ApplicationUser>(null));

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            var result = await controller.Follows(UserName);

            Assert.NotNull(result);
            Assert.IsType<NotFoundResult>(result.Result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task Follows_NotFoundUserId()
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
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            var result = await controller.Follows(UserName);

            Assert.NotNull(result);
            Assert.IsType<NotFoundResult>(result.Result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task AddFollow_Success()
        {
            const string UserName = "SomeUserName";
            const string UserId = "SomeUserId";
            const string FollowUserId = "SomeFollowUserId";

            var model = new AddFollowRequest
            {
                FollowUserName = "SomeFollowUserName",
            };

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();

            var mockDatabaseCommandParameters = new Dictionary<string, object>() { { "@UserId", UserId }, { "@FollowUserId", FollowUserId } };
            var mockDatabaseCommand = MockDatabaseHelper.CreateMockDatabaseCommand(mockDatabaseCommandParameters);

            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            mockDatabaseCommandFactory
                .Setup(_ => _.Create())
                .Returns(mockDatabaseCommand.Object);

            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
            var mockUser = new ApplicationUser();
            var mockFollowUser = new ApplicationUser();
            var mockCurrentUser = new Mock<ClaimsPrincipal>(MockBehavior.Strict);
            var mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult(mockUser));
            mockUserManager
                .Setup(_ => _.FindByNameAsync(model.FollowUserName))
                .Returns(Task.FromResult(mockFollowUser));
            mockUserManager
                .Setup(_ => _.GetUserIdAsync(mockUser))
                .Returns(Task.FromResult(UserId));
            mockUserManager
                .Setup(_ => _.GetUserIdAsync(mockFollowUser))
                .Returns(Task.FromResult(FollowUserId));
            mockUserManager
                .Setup(_ => _.GetUserId(mockCurrentUser.Object))
                .Returns(UserId);

            var mockHttpContext = new Mock<HttpContext>(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            var result = await controller.AddFollow(UserName, model);

            Assert.NotNull(result);
            Assert.IsType<OkResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockHttpContext.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task AddFollow_NotFoundUser()
        {
            const string UserName = "SomeUserName";

            var model = new AddFollowRequest
            {
                FollowUserName = "SomeFollowUserName",
            };

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
            var mockCurrentUser = new Mock<ClaimsPrincipal>(MockBehavior.Strict);
            var mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult<ApplicationUser>(null));

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            var result = await controller.AddFollow(UserName, model);

            Assert.NotNull(result);
            Assert.IsType<NotFoundResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task AddFollow_NotFoundUserId()
        {
            const string UserName = "SomeUserName";

            var model = new AddFollowRequest
            {
                FollowUserName = "SomeFollowUserName",
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
                .Returns(Task.FromResult<string>(null));

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            var result = await controller.AddFollow(UserName, model);

            Assert.NotNull(result);
            Assert.IsType<NotFoundResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task AddFollow_NotFoundFollowUser()
        {
            const string UserName = "SomeUserName";
            const string UserId = "SomeUserId";

            var model = new AddFollowRequest
            {
                FollowUserName = "SomeFollowUserName",
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
                .Setup(_ => _.FindByNameAsync(model.FollowUserName))
                .Returns(Task.FromResult<ApplicationUser>(null));
            mockUserManager
                .Setup(_ => _.GetUserIdAsync(mockUser))
                .Returns(Task.FromResult(UserId));
            mockUserManager
                .Setup(_ => _.GetUserId(mockCurrentUser.Object))
                .Returns(UserId);

            var mockHttpContext = new Mock<HttpContext>(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };
            var result = await controller.AddFollow(UserName, model);

            Assert.NotNull(result);
            Assert.IsType<NotFoundResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task AddFollow_NotFoundFollowUserId()
        {
            const string UserName = "SomeUserName";
            const string UserId = "SomeUserId";

            var model = new AddFollowRequest
            {
                FollowUserName = "SomeFollowUserName",
            };

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
            var mockUser = new ApplicationUser();
            var mockFollowUser = new ApplicationUser();
            var mockCurrentUser = new Mock<ClaimsPrincipal>(MockBehavior.Strict);
            var mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult(mockUser));
            mockUserManager
                .Setup(_ => _.FindByNameAsync(model.FollowUserName))
                .Returns(Task.FromResult(mockFollowUser));
            mockUserManager
                .Setup(_ => _.GetUserIdAsync(mockUser))
                .Returns(Task.FromResult(UserId));
            mockUserManager
                .Setup(_ => _.GetUserIdAsync(mockFollowUser))
                .Returns(Task.FromResult<string>(null));
            mockUserManager
                .Setup(_ => _.GetUserId(mockCurrentUser.Object))
                .Returns(UserId);

            var mockHttpContext = new Mock<HttpContext>(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            var result = await controller.AddFollow(UserName, model);

            Assert.NotNull(result);
            Assert.IsType<NotFoundResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task AddFollow_NotAllowed()
        {
            const string UserName = "SomeUserName";
            const string UserId = "SomeUserId";

            var model = new AddFollowRequest
            {
                FollowUserName = "SomeFollowUserName",
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
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            var result = await controller.AddFollow(UserName, model);

            Assert.NotNull(result);
            Assert.IsType<ForbidResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockHttpContext.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task AddFollow_AdminIsAlwaysAllowed()
        {
            const string UserName = "SomeUserName";
            const string UserId = "SomeUserId";
            const string FollowUserId = "SomeFollowUserId";

            var model = new AddFollowRequest
            {
                FollowUserName = "SomeFollowUserName",
            };

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();

            var mockDatabaseCommandParameters = new Dictionary<string, object>() { { "@UserId", UserId }, { "@FollowUserId", FollowUserId } };
            var mockDatabaseCommand = MockDatabaseHelper.CreateMockDatabaseCommand(mockDatabaseCommandParameters);

            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            mockDatabaseCommandFactory
                .Setup(_ => _.Create())
                .Returns(mockDatabaseCommand.Object);

            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
            var mockUser = new ApplicationUser();
            var mockFollowUser = new ApplicationUser();
            var mockCurrentUser = new Mock<ClaimsPrincipal>(MockBehavior.Strict);
            mockCurrentUser
                .Setup(_ => _.IsInRole("Admin"))
                .Returns(true);

            var mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult(mockUser));
            mockUserManager
                .Setup(_ => _.FindByNameAsync(model.FollowUserName))
                .Returns(Task.FromResult(mockFollowUser));
            mockUserManager
                .Setup(_ => _.GetUserIdAsync(mockUser))
                .Returns(Task.FromResult(UserId));
            mockUserManager
                .Setup(_ => _.GetUserIdAsync(mockFollowUser))
                .Returns(Task.FromResult(FollowUserId));
            mockUserManager
                .Setup(_ => _.GetUserId(mockCurrentUser.Object))
                .Returns("SomeOtherUserId");

            var mockHttpContext = new Mock<HttpContext>(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            var result = await controller.AddFollow(UserName, model);

            Assert.NotNull(result);
            Assert.IsType<OkResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockHttpContext.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task RemoveFollow_Success()
        {
            const string UserName = "SomeUserName";
            const string UserId = "SomeUserId";
            const string FollowUserName = "SomeFollowUserName";
            const string FollowUserId = "SomeFollowUserId";

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();

            var mockDatabaseCommandParameters = new Dictionary<string, object>() { { "@UserId", UserId }, { "@FollowUserId", FollowUserId } };
            var mockDatabaseCommand = MockDatabaseHelper.CreateMockDatabaseCommand(mockDatabaseCommandParameters, 1);

            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            mockDatabaseCommandFactory
                .Setup(_ => _.Create())
                .Returns(mockDatabaseCommand.Object);

            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
            var mockUser = new ApplicationUser();
            var mockFollowUser = new ApplicationUser();
            var mockCurrentUser = new Mock<ClaimsPrincipal>(MockBehavior.Strict);
            var mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult(mockUser));
            mockUserManager
                .Setup(_ => _.FindByNameAsync(FollowUserName))
                .Returns(Task.FromResult(mockFollowUser));
            mockUserManager
                .Setup(_ => _.GetUserIdAsync(mockUser))
                .Returns(Task.FromResult(UserId));
            mockUserManager
                .Setup(_ => _.GetUserIdAsync(mockFollowUser))
                .Returns(Task.FromResult(FollowUserId));
            mockUserManager
                .Setup(_ => _.GetUserId(mockCurrentUser.Object))
                .Returns(UserId);

            var mockHttpContext = new Mock<HttpContext>(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            var result = await controller.RemoveFollow(UserName, FollowUserName);

            Assert.NotNull(result);
            Assert.IsType<OkResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockHttpContext.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task RemoveFollow_NotFoundUser()
        {
            const string UserName = "SomeUserName";
            const string FollowUserName = "SomeFollowUserName";

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
            var mockCurrentUser = new Mock<ClaimsPrincipal>(MockBehavior.Strict);
            var mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult<ApplicationUser>(null));

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            var result = await controller.RemoveFollow(UserName, FollowUserName);

            Assert.NotNull(result);
            Assert.IsType<NotFoundResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task RemoveFollow_NotFoundUserId()
        {
            const string UserName = "SomeUserName";
            const string FollowUserName = "SomeFollowUserName";

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
                .Returns(Task.FromResult<string>(null));

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            var result = await controller.RemoveFollow(UserName, FollowUserName);

            Assert.NotNull(result);
            Assert.IsType<NotFoundResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task RemoveFollow_NotFoundFollowUser()
        {
            const string UserName = "SomeUserName";
            const string UserId = "SomeUserId";
            const string FollowUserName = "SomeFollowUserName";

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
                .Setup(_ => _.FindByNameAsync(FollowUserName))
                .Returns(Task.FromResult<ApplicationUser>(null));
            mockUserManager
                .Setup(_ => _.GetUserIdAsync(mockUser))
                .Returns(Task.FromResult(UserId));
            mockUserManager
                .Setup(_ => _.GetUserId(mockCurrentUser.Object))
                .Returns(UserId);

            var mockHttpContext = new Mock<HttpContext>(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };
            var result = await controller.RemoveFollow(UserName, FollowUserName);

            Assert.NotNull(result);
            Assert.IsType<NotFoundResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task RemoveFollow_NotFoundFollowUserId()
        {
            const string UserName = "SomeUserName";
            const string UserId = "SomeUserId";
            const string FollowUserName = "SomeFollowUserName";

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
            var mockUser = new ApplicationUser();
            var mockFollowUser = new ApplicationUser();
            var mockCurrentUser = new Mock<ClaimsPrincipal>(MockBehavior.Strict);
            var mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult(mockUser));
            mockUserManager
                .Setup(_ => _.FindByNameAsync(FollowUserName))
                .Returns(Task.FromResult(mockFollowUser));
            mockUserManager
                .Setup(_ => _.GetUserIdAsync(mockUser))
                .Returns(Task.FromResult(UserId));
            mockUserManager
                .Setup(_ => _.GetUserIdAsync(mockFollowUser))
                .Returns(Task.FromResult<string>(null));
            mockUserManager
                .Setup(_ => _.GetUserId(mockCurrentUser.Object))
                .Returns(UserId);

            var mockHttpContext = new Mock<HttpContext>(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            var result = await controller.RemoveFollow(UserName, FollowUserName);

            Assert.NotNull(result);
            Assert.IsType<NotFoundResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task RemoveFollow_NotAllowed()
        {
            const string UserName = "SomeUserName";
            const string UserId = "SomeUserId";
            const string FollowUserName = "SomeFollowUserName";

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
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            var result = await controller.RemoveFollow(UserName, FollowUserName);

            Assert.NotNull(result);
            Assert.IsType<ForbidResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockHttpContext.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task RemoveFollow_AdminIsAlwaysAllowed()
        {
            const string UserName = "SomeUserName";
            const string UserId = "SomeUserId";
            const string FollowUserName = "SomeFollowUserName";
            const string FollowUserId = "SomeFollowUserId";

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();

            var mockDatabaseCommandParameters = new Dictionary<string, object>() { { "@UserId", UserId }, { "@FollowUserId", FollowUserId } };
            var mockDatabaseCommand = MockDatabaseHelper.CreateMockDatabaseCommand(mockDatabaseCommandParameters, 1);

            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            mockDatabaseCommandFactory
                .Setup(_ => _.Create())
                .Returns(mockDatabaseCommand.Object);

            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
            var mockUser = new ApplicationUser();
            var mockFollowUser = new ApplicationUser();
            var mockCurrentUser = new Mock<ClaimsPrincipal>(MockBehavior.Strict);
            mockCurrentUser
                .Setup(_ => _.IsInRole("Admin"))
                .Returns(true);

            var mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult(mockUser));
            mockUserManager
                .Setup(_ => _.FindByNameAsync(FollowUserName))
                .Returns(Task.FromResult(mockFollowUser));
            mockUserManager
                .Setup(_ => _.GetUserIdAsync(mockUser))
                .Returns(Task.FromResult(UserId));
            mockUserManager
                .Setup(_ => _.GetUserIdAsync(mockFollowUser))
                .Returns(Task.FromResult(FollowUserId));
            mockUserManager
                .Setup(_ => _.GetUserId(mockCurrentUser.Object))
                .Returns("SomeOtherUserId");

            var mockHttpContext = new Mock<HttpContext>(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            var result = await controller.RemoveFollow(UserName, FollowUserName);

            Assert.NotNull(result);
            Assert.IsType<OkResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockHttpContext.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task RemoveFollow_NoDeletion()
        {
            const string UserName = "SomeUserName";
            const string UserId = "SomeUserId";
            const string FollowUserName = "SomeFollowUserName";
            const string FollowUserId = "SomeFollowUserId";

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();

            var mockDatabaseCommandParameters = new Dictionary<string, object>() { { "@UserId", UserId }, { "@FollowUserId", FollowUserId } };
            var mockDatabaseCommand = MockDatabaseHelper.CreateMockDatabaseCommand(mockDatabaseCommandParameters, 0);

            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            mockDatabaseCommandFactory
                .Setup(_ => _.Create())
                .Returns(mockDatabaseCommand.Object);

            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
            var mockUser = new ApplicationUser();
            var mockFollowUser = new ApplicationUser();
            var mockCurrentUser = new Mock<ClaimsPrincipal>(MockBehavior.Strict);
            var mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult(mockUser));
            mockUserManager
                .Setup(_ => _.FindByNameAsync(FollowUserName))
                .Returns(Task.FromResult(mockFollowUser));
            mockUserManager
                .Setup(_ => _.GetUserIdAsync(mockUser))
                .Returns(Task.FromResult(UserId));
            mockUserManager
                .Setup(_ => _.GetUserIdAsync(mockFollowUser))
                .Returns(Task.FromResult(FollowUserId));
            mockUserManager
                .Setup(_ => _.GetUserId(mockCurrentUser.Object))
                .Returns(UserId);

            var mockHttpContext = new Mock<HttpContext>(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            var result = await controller.RemoveFollow(UserName, FollowUserName);

            Assert.NotNull(result);
            Assert.IsType<NotFoundResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockHttpContext.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task GetSettings_Success()
        {
            const string UserName = "SomeUserName";
            const string UserId = "SomeUserId";

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);

            var userSettings = new UserSettings();
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
            mockUserSettingsProvider
                .Setup(_ => _.GetAsync(UserId))
                .Returns(Task.FromResult(userSettings));

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
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            var result = await controller.GetSettings(UserName);
            Assert.NotNull(result);
            Assert.Equal(userSettings, result.Value);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockHttpContext.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task GetSettings_NotFoundUser()
        {
            const string UserName = "SomeUserName";

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
            var mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult<ApplicationUser>(null));

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);

            var result = await controller.GetSettings(UserName);

            Assert.NotNull(result);
            Assert.IsType<NotFoundResult>(result.Result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task GetSettings_NotFoundUserId()
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
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);

            var result = await controller.GetSettings(UserName);

            Assert.NotNull(result);
            Assert.IsType<NotFoundResult>(result.Result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task GetSettings_NotAllowed()
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
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            var result = await controller.GetSettings(UserName);

            Assert.NotNull(result);
            Assert.IsType<ForbidResult>(result.Result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockHttpContext.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task GetSettings_AdminIsAlwaysAllowed()
        {
            const string UserName = "SomeUserName";
            const string UserId = "SomeUserId";

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);

            var userSettings = new UserSettings();
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
            mockUserSettingsProvider
                .Setup(_ => _.GetAsync(UserId))
                .Returns(Task.FromResult(userSettings));

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
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            var result = await controller.GetSettings(UserName);
            Assert.NotNull(result);
            Assert.Equal(userSettings, result.Value);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockHttpContext.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task PatchSettings_Success()
        {
            const string UserName = "SomeUserName";
            const string UserId = "SomeUserId";

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var userSettings = new UserSettings();
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
            mockUserSettingsProvider
                .Setup(_ => _.PatchAsync(UserId, userSettings))
                .Returns(Task.CompletedTask);

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
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            var result = await controller.PatchSettings(UserName, userSettings);

            Assert.NotNull(result);
            Assert.IsType<OkResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockHttpContext.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task PatchSettings_NotFoundUser()
        {
            const string UserName = "SomeUserName";

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var userSettings = new UserSettings();
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
            var mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult<ApplicationUser>(null));

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);

            var result = await controller.PatchSettings(UserName, userSettings);

            Assert.NotNull(result);
            Assert.IsType<NotFoundResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task PatchSettings_NotFoundUserId()
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
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);

            var result = await controller.PatchSettings(UserName, userSettings);

            Assert.NotNull(result);
            Assert.IsType<NotFoundResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task PatchSettings_NotAllowed()
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
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            var result = await controller.PatchSettings(UserName, userSettings);

            Assert.NotNull(result);
            Assert.IsType<ForbidResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockHttpContext.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task GetLogins_Success()
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

            var logins = new List<ExternalLogin>
            {
                new ExternalLogin { ProviderName = "SomeProvider0", ExternalUserId = "SomeExternalUserId0" },
                new ExternalLogin { ProviderName = "SomeProvider1", ExternalUserId = "SomeExternalUserId1" },
                new ExternalLogin { ProviderName = "SomeProvider2", ExternalUserId = "SomeExternalUserId2" },
            };
            mockUserManager
                .Setup(_ => _.GetLoginsAsync(mockUser))
                .Returns(Task.FromResult<IList<UserLoginInfo>>(logins.Select(login => new UserLoginInfo(login.ProviderName, login.ExternalUserId, login.ProviderName)).ToList()));

            var mockHttpContext = new Mock<HttpContext>(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            var result = await controller.GetLogins(UserName);
            Assert.NotNull(result);

            var model = result.Value;
            Assert.True(model.HasPassword);

            Assert.Equal(logins.Count, model.ExternalLogins.Count);
            for (var i = 0; i < model.ExternalLogins.Count; i++)
            {
                Assert.Equal(logins[i].ProviderName, model.ExternalLogins[i].ProviderName);
                Assert.Equal(logins[i].ExternalUserId, model.ExternalLogins[i].ExternalUserId);
            }

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockHttpContext.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task GetLogins_NotFoundUser()
        {
            const string UserName = "SomeUserName";

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
            var mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult<ApplicationUser>(null));

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);

            var result = await controller.GetLogins(UserName);

            Assert.NotNull(result);
            Assert.IsType<NotFoundResult>(result.Result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task GetLogins_NotFoundUserId()
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
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);

            var result = await controller.GetLogins(UserName);

            Assert.NotNull(result);
            Assert.IsType<NotFoundResult>(result.Result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task GetLogins_NotAllowed()
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
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            var result = await controller.GetLogins(UserName);

            Assert.NotNull(result);
            Assert.IsType<ForbidResult>(result.Result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockHttpContext.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task GetLogins_AdminIsAlwaysAllowed()
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

            var logins = new List<ExternalLogin>
            {
                new ExternalLogin { ProviderName = "SomeProvider0", ExternalUserId = "SomeExternalUserId0" },
                new ExternalLogin { ProviderName = "SomeProvider1", ExternalUserId = "SomeExternalUserId1" },
                new ExternalLogin { ProviderName = "SomeProvider2", ExternalUserId = "SomeExternalUserId2" },
            };
            mockUserManager
                .Setup(_ => _.GetLoginsAsync(mockUser))
                .Returns(Task.FromResult<IList<UserLoginInfo>>(logins.Select(login => new UserLoginInfo(login.ProviderName, login.ExternalUserId, login.ProviderName)).ToList()));

            var mockHttpContext = new Mock<HttpContext>(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            var result = await controller.GetLogins(UserName);
            Assert.NotNull(result);

            var model = result.Value;
            Assert.True(model.HasPassword);

            Assert.Equal(logins.Count, model.ExternalLogins.Count);
            for (var i = 0; i < model.ExternalLogins.Count; i++)
            {
                Assert.Equal(logins[i].ProviderName, model.ExternalLogins[i].ProviderName);
                Assert.Equal(logins[i].ExternalUserId, model.ExternalLogins[i].ExternalUserId);
            }

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockHttpContext.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task RemoveLogin_Success()
        {
            const string UserName = "SomeUserName";
            const string UserId = "SomeUserId";
            const string Provider = "SomeProvider";
            const string ExternalUserId = "SomeExternalUserId";

            var model = new ExternalLogin
            {
                ProviderName = Provider,
                ExternalUserId = ExternalUserId,
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
                .Setup(_ => _.RemoveLoginAsync(mockUser, Provider, ExternalUserId))
                .Returns(Task.FromResult(IdentityResult.Success));

            var mockHttpContext = new Mock<HttpContext>(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            var result = await controller.RemoveLogin(UserName, model);

            Assert.NotNull(result);
            Assert.IsType<OkResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockHttpContext.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task RemoveLogin_NotFoundUser()
        {
            const string UserName = "SomeUserName";
            const string Provider = "SomeProvider";
            const string ExternalUserId = "SomeExternalUserId";

            var model = new ExternalLogin
            {
                ProviderName = Provider,
                ExternalUserId = ExternalUserId,
            };

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
            var mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult<ApplicationUser>(null));

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);

            var result = await controller.RemoveLogin(UserName, model);

            Assert.NotNull(result);
            Assert.IsType<NotFoundResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task RemoveLogin_NotFoundUserId()
        {
            const string UserName = "SomeUserName";
            const string Provider = "SomeProvider";
            const string ExternalUserId = "SomeExternalUserId";

            var model = new ExternalLogin
            {
                ProviderName = Provider,
                ExternalUserId = ExternalUserId,
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
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);

            var result = await controller.RemoveLogin(UserName, model);

            Assert.NotNull(result);
            Assert.IsType<NotFoundResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task RemoveLogin_NotAllowed()
        {
            const string UserName = "SomeUserName";
            const string UserId = "SomeUserId";
            const string Provider = "SomeProvider";
            const string ExternalUserId = "SomeExternalUserId";

            var model = new ExternalLogin
            {
                ProviderName = Provider,
                ExternalUserId = ExternalUserId,
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
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            var result = await controller.RemoveLogin(UserName, model);

            Assert.NotNull(result);
            Assert.IsType<ForbidResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockHttpContext.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task RemoveLogin_AdminIsAlwaysAllowed()
        {
            const string UserName = "SomeUserName";
            const string UserId = "SomeUserId";
            const string Provider = "SomeProvider";
            const string ExternalUserId = "SomeExternalUserId";

            var model = new ExternalLogin
            {
                ProviderName = Provider,
                ExternalUserId = ExternalUserId,
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
                .Setup(_ => _.RemoveLoginAsync(mockUser, Provider, ExternalUserId))
                .Returns(Task.FromResult(IdentityResult.Success));

            var mockHttpContext = new Mock<HttpContext>(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            var result = await controller.RemoveLogin(UserName, model);

            Assert.NotNull(result);
            Assert.IsType<OkResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockHttpContext.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task RemoveLogin_RemoveLoginFails()
        {
            const string UserName = "SomeUserName";
            const string UserId = "SomeUserId";
            const string Provider = "SomeProvider";
            const string ExternalUserId = "SomeExternalUserId";

            var model = new ExternalLogin
            {
                ProviderName = Provider,
                ExternalUserId = ExternalUserId,
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
                .Setup(_ => _.RemoveLoginAsync(mockUser, Provider, ExternalUserId))
                .Returns(Task.FromResult(IdentityResult.Failed(new IdentityError { Description = "SomeDescription" })));

            var mockHttpContext = new Mock<HttpContext>(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            var result = await controller.RemoveLogin(UserName, model);

            Assert.NotNull(result);
            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(1, controller.ModelState.ErrorCount);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockHttpContext.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task PatchSettings_AdminIsAlwaysAllowed()
        {
            const string UserName = "SomeUserName";
            const string UserId = "SomeUserId";

            var gameData = MockGameData.RealData;
            var telemetryClient = new TelemetryClient();
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var userSettings = new UserSettings();
            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
            mockUserSettingsProvider
                .Setup(_ => _.PatchAsync(UserId, userSettings))
                .Returns(Task.CompletedTask);

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
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            var result = await controller.PatchSettings(UserName, userSettings);

            Assert.NotNull(result);
            Assert.IsType<OkResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockHttpContext.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task SetPassword_Success()
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
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            var result = await controller.SetPassword(UserName, model);

            Assert.NotNull(result);
            Assert.IsType<OkResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockHttpContext.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task SetPassword_NotFoundUser()
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
            var mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult<ApplicationUser>(null));

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);

            var result = await controller.SetPassword(UserName, model);

            Assert.NotNull(result);
            Assert.IsType<NotFoundResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task SetPassword_NotFoundUserId()
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
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);

            var result = await controller.SetPassword(UserName, model);

            Assert.NotNull(result);
            Assert.IsType<NotFoundResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task SetPassword_NotAllowed()
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
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            var result = await controller.SetPassword(UserName, model);

            Assert.NotNull(result);
            Assert.IsType<ForbidResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockHttpContext.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task SetPassword_AdminIsAlwaysAllowed()
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
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            var result = await controller.SetPassword(UserName, model);

            Assert.NotNull(result);
            Assert.IsType<OkResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockHttpContext.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task SetPassword_SetPasswordFails()
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
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
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
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task ChangePassword_Success()
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
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            var result = await controller.ChangePassword(UserName, model);

            Assert.NotNull(result);
            Assert.IsType<OkResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockHttpContext.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task ChangePassword_NotFoundUser()
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
            var mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult<ApplicationUser>(null));

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);

            var result = await controller.ChangePassword(UserName, model);

            Assert.NotNull(result);
            Assert.IsType<NotFoundResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task ChangePassword_NotFoundUserId()
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
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);

            var result = await controller.ChangePassword(UserName, model);

            Assert.NotNull(result);
            Assert.IsType<NotFoundResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task ChangePassword_NotAllowed()
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
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            var result = await controller.ChangePassword(UserName, model);

            Assert.NotNull(result);
            Assert.IsType<ForbidResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockHttpContext.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task ChangePassword_AdminIsAlwaysAllowed()
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
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            var result = await controller.ChangePassword(UserName, model);

            Assert.NotNull(result);
            Assert.IsType<OkResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockCurrentUser.VerifyAll();
            mockHttpContext.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task ChangePassword_ChangePasswordFails()
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
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
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
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task ResetPassword_Success()
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
                .Setup(_ => _.SendEmailAsync(model.Email, It.IsAny<string>(), It.Is<string>(str => str.Contains(Code, StringComparison.Ordinal))))
                .Returns(Task.CompletedTask);

            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

            var controller = new UserController(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            var result = await controller.ResetPassword(model);

            Assert.NotNull(result);
            Assert.IsType<OkResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task ResetPassword_UserLookupFails()
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
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

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
                mockEmailSender.Object,
                mockClanManager.Object);
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
        public static async Task ResetPasswordConfirmation_Success()
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
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

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
                mockEmailSender.Object,
                mockClanManager.Object);
            var result = await controller.ResetPasswordConfirmation(model);

            Assert.NotNull(result);
            Assert.IsType<OkResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUserSettingsProvider.VerifyAll();
            mockEmailSender.VerifyAll();
            mockClanManager.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public static async Task ResetPasswordConfirmation_UserLookupFails()
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
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

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
                mockEmailSender.Object,
                mockClanManager.Object);
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
        public static async Task ResetPasswordConfirmation_ResetPasswordFails()
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
            var mockClanManager = new Mock<IClanManager>(MockBehavior.Strict);

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
                mockEmailSender.Object,
                mockClanManager.Object);
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
    }
}