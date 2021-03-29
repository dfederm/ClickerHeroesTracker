// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ClickerHeroesTrackerWebsite.Models;
using ClickerHeroesTrackerWebsite.Models.Api.Uploads;
using ClickerHeroesTrackerWebsite.Models.Game;
using ClickerHeroesTrackerWebsite.Models.Settings;
using ClickerHeroesTrackerWebsite.Services.Database;
using ClickerHeroesTrackerWebsite.Services.Email;
using ClickerHeroesTrackerWebsite.Tests.Mocks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Website.Controllers;
using Website.Models.Api.Users;
using Website.Services.Clans;
using Xunit;

namespace UnitTests.Controllers
{
    public static class UserControllerTests
    {
        [Fact]
        public static async Task Create_Success()
        {
            CreateUserRequest createUser = new()
            {
                UserName = "SomeUserName",
                Email = "SomeEmail",
                Password = "SomePassword",
            };

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);
            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.CreateAsync(It.Is<ApplicationUser>(user => user.UserName == createUser.UserName && user.Email == createUser.Email), createUser.Password))
                .Returns(Task.FromResult(IdentityResult.Success));
            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            ActionResult result = await controller.CreateAsync(createUser);

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
            CreateUserRequest createUser = new()
            {
                UserName = "SomeUserName",
                Email = "SomeEmail",
                Password = "SomePassword",
            };

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);
            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.CreateAsync(It.Is<ApplicationUser>(user => user.UserName == createUser.UserName && user.Email == createUser.Email), createUser.Password))
                .Returns(Task.FromResult(IdentityResult.Failed(new IdentityError { Description = "SomeDescription" })));
            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            ActionResult result = await controller.CreateAsync(createUser);

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

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);

            ApplicationUser mockUser = new() { UserName = UserName };
            Mock<ClaimsPrincipal> mockCurrentUser = new(MockBehavior.Strict);

            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult(mockUser));
            mockUserManager
                .Setup(_ => _.GetUserIdAsync(mockUser))
                .Returns(Task.FromResult(UserId));

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);

            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);
            mockClanManager
                .Setup(_ => _.GetClanNameAsync(UserId))
                .Returns(Task.FromResult(ClanName));

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);

            ActionResult<User> result = await controller.GetAsync(UserName);
            Assert.NotNull(result);

            User model = result.Value;
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

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);
            Mock<ClaimsPrincipal> mockCurrentUser = new(MockBehavior.Strict);

            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult<ApplicationUser>(null));

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);

            ActionResult<User> result = await controller.GetAsync(UserName);

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

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);

            ApplicationUser mockUser = new() { UserName = UserName };
            Mock<ClaimsPrincipal> mockCurrentUser = new(MockBehavior.Strict);

            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult(mockUser));
            mockUserManager
                .Setup(_ => _.GetUserIdAsync(mockUser))
                .Returns(Task.FromResult<string>(null));

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);

            ActionResult<User> result = await controller.GetAsync(UserName);

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

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());

            List<int> expectedUploadIds = Enumerable.Range(0, 3).ToList();
            List<IDictionary<string, object>> getUploadsDatasets = expectedUploadIds
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
            Mock<IDataReader> mockGetUploadsDataReader = MockDatabaseHelper.CreateMockDataReader(getUploadsDatasets);
            Dictionary<string, object> mockGetUploadsDatabaseCommandParameters = new() { { "@UserId", UserId }, { "@Offset", (Page - 1) * Count }, { "@Count", Count } };
            Mock<IDatabaseCommand> mockGetUploadsDatabaseCommand = MockDatabaseHelper.CreateMockDatabaseCommand(mockGetUploadsDatabaseCommandParameters, mockGetUploadsDataReader.Object);

            Dictionary<string, object> paginationDataset = new() { { "TotalUploads", TotalUploads } };
            Mock<IDataReader> mockPaginationDataReader = MockDatabaseHelper.CreateMockDataReader(paginationDataset);
            Dictionary<string, object> mockPaginationDatabaseCommandParameters = new() { { "@UserId", UserId } };
            Mock<IDatabaseCommand> mockPaginationDatabaseCommand = MockDatabaseHelper.CreateMockDatabaseCommand(mockPaginationDatabaseCommandParameters, mockPaginationDataReader.Object);

            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            int commandCreationCount = 0;
            mockDatabaseCommandFactory
                .Setup(_ => _.Create())
                .Returns(() =>
                {
                    return commandCreationCount++ switch
                    {
                        0 => mockGetUploadsDatabaseCommand.Object,
                        1 => mockPaginationDatabaseCommand.Object,
                        _ => throw new InvalidOperationException("Unexpected call to DatabaseCommandFactory.Create"),
                    };
                });

            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);
            ApplicationUser mockUser = new();
            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult(mockUser));
            mockUserManager
                .Setup(_ => _.GetUserIdAsync(mockUser))
                .Returns(Task.FromResult(UserId));

            // Use loose behavior since this is accessed a bunch internally in a controller
            Mock<HttpRequest> mockHttpRequest = new(MockBehavior.Loose);
            mockHttpRequest.SetupGet(_ => _.Path).Returns(RequestPath);

            Mock<HttpContext> mockHttpContext = new(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.Request).Returns(mockHttpRequest.Object);

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            ActionResult<UploadSummaryListResponse> result = await controller.UploadsAsync(UserName, Page, Count);
            Assert.NotNull(result);

            UploadSummaryListResponse model = result.Value;
            Assert.NotNull(model);

            Assert.NotNull(model.Uploads);
            Assert.Equal(expectedUploadIds.Count, model.Uploads.Count);
            for (int i = 0; i < model.Uploads.Count; i++)
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
            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);
            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);

            ActionResult<UploadSummaryListResponse> result = await controller.UploadsAsync(userName, page, count);

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

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);

            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult<ApplicationUser>(null));

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);

            ActionResult<UploadSummaryListResponse> result = await controller.UploadsAsync(UserName, Page, Count);

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

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);
            ApplicationUser mockUser = new();

            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult(mockUser));
            mockUserManager
                .Setup(_ => _.GetUserIdAsync(mockUser))
                .Returns(Task.FromResult<string>(null));

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            ActionResult<UploadSummaryListResponse> result = await controller.UploadsAsync(UserName, Page, Count);

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

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());

            List<string> follows = Enumerable.Range(0, 3)
                .Select(i => "SomeUser" + i)
                .ToList();
            List<IDictionary<string, object>> datasets = follows
                .Select<string, IDictionary<string, object>>(follow => new Dictionary<string, object> { { "UserName", follow } })
                .ToList();
            Mock<IDataReader> mockDataReader = MockDatabaseHelper.CreateMockDataReader(datasets);
            Dictionary<string, object> mockDatabaseCommandParameters = new() { { "@UserId", UserId } };
            Mock<IDatabaseCommand> mockDatabaseCommand = MockDatabaseHelper.CreateMockDatabaseCommand(mockDatabaseCommandParameters, mockDataReader.Object);

            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            mockDatabaseCommandFactory
                .Setup(_ => _.Create())
                .Returns(mockDatabaseCommand.Object);

            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);
            ApplicationUser mockUser = new();
            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult(mockUser));
            mockUserManager
                .Setup(_ => _.GetUserIdAsync(mockUser))
                .Returns(Task.FromResult(UserId));

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);

            ActionResult<FollowsData> result = await controller.FollowsAsync(UserName);
            Assert.NotNull(result);

            FollowsData model = result.Value;
            Assert.NotNull(model);
            Assert.Equal(follows.Count, model.Follows.Count);
            for (int i = 0; i < model.Follows.Count; i++)
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

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);
            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult<ApplicationUser>(null));

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            ActionResult<FollowsData> result = await controller.FollowsAsync(UserName);

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

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);
            ApplicationUser mockUser = new();
            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult(mockUser));
            mockUserManager
                .Setup(_ => _.GetUserIdAsync(mockUser))
                .Returns(Task.FromResult<string>(null));

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            ActionResult<FollowsData> result = await controller.FollowsAsync(UserName);

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

            AddFollowRequest model = new()
            {
                FollowUserName = "SomeFollowUserName",
            };

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());

            Dictionary<string, object> mockDatabaseCommandParameters = new() { { "@UserId", UserId }, { "@FollowUserId", FollowUserId } };
            Mock<IDatabaseCommand> mockDatabaseCommand = MockDatabaseHelper.CreateMockDatabaseCommand(mockDatabaseCommandParameters);

            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            mockDatabaseCommandFactory
                .Setup(_ => _.Create())
                .Returns(mockDatabaseCommand.Object);

            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);
            ApplicationUser mockUser = new();
            ApplicationUser mockFollowUser = new();
            Mock<ClaimsPrincipal> mockCurrentUser = new(MockBehavior.Strict);
            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
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

            Mock<HttpContext> mockHttpContext = new(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            ActionResult result = await controller.AddFollowAsync(UserName, model);

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

            AddFollowRequest model = new()
            {
                FollowUserName = "SomeFollowUserName",
            };

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);
            Mock<ClaimsPrincipal> mockCurrentUser = new(MockBehavior.Strict);
            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult<ApplicationUser>(null));

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            ActionResult result = await controller.AddFollowAsync(UserName, model);

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

            AddFollowRequest model = new()
            {
                FollowUserName = "SomeFollowUserName",
            };

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);
            ApplicationUser mockUser = new();
            Mock<ClaimsPrincipal> mockCurrentUser = new(MockBehavior.Strict);
            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult(mockUser));
            mockUserManager
                .Setup(_ => _.GetUserIdAsync(mockUser))
                .Returns(Task.FromResult<string>(null));

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            ActionResult result = await controller.AddFollowAsync(UserName, model);

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

            AddFollowRequest model = new()
            {
                FollowUserName = "SomeFollowUserName",
            };

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);
            ApplicationUser mockUser = new();
            Mock<ClaimsPrincipal> mockCurrentUser = new(MockBehavior.Strict);
            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
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

            Mock<HttpContext> mockHttpContext = new(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };
            ActionResult result = await controller.AddFollowAsync(UserName, model);

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

            AddFollowRequest model = new()
            {
                FollowUserName = "SomeFollowUserName",
            };

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);
            ApplicationUser mockUser = new();
            ApplicationUser mockFollowUser = new();
            Mock<ClaimsPrincipal> mockCurrentUser = new(MockBehavior.Strict);
            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
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

            Mock<HttpContext> mockHttpContext = new(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            ActionResult result = await controller.AddFollowAsync(UserName, model);

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

            AddFollowRequest model = new()
            {
                FollowUserName = "SomeFollowUserName",
            };

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);
            ApplicationUser mockUser = new();
            Mock<ClaimsPrincipal> mockCurrentUser = new(MockBehavior.Strict);
            mockCurrentUser
                .Setup(_ => _.IsInRole("Admin"))
                .Returns(false);

            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult(mockUser));
            mockUserManager
                .Setup(_ => _.GetUserIdAsync(mockUser))
                .Returns(Task.FromResult(UserId));
            mockUserManager
                .Setup(_ => _.GetUserId(mockCurrentUser.Object))
                .Returns("SomeOtherUserId");

            Mock<HttpContext> mockHttpContext = new(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            ActionResult result = await controller.AddFollowAsync(UserName, model);

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

            AddFollowRequest model = new()
            {
                FollowUserName = "SomeFollowUserName",
            };

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());

            Dictionary<string, object> mockDatabaseCommandParameters = new() { { "@UserId", UserId }, { "@FollowUserId", FollowUserId } };
            Mock<IDatabaseCommand> mockDatabaseCommand = MockDatabaseHelper.CreateMockDatabaseCommand(mockDatabaseCommandParameters);

            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            mockDatabaseCommandFactory
                .Setup(_ => _.Create())
                .Returns(mockDatabaseCommand.Object);

            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);
            ApplicationUser mockUser = new();
            ApplicationUser mockFollowUser = new();
            Mock<ClaimsPrincipal> mockCurrentUser = new(MockBehavior.Strict);
            mockCurrentUser
                .Setup(_ => _.IsInRole("Admin"))
                .Returns(true);

            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
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

            Mock<HttpContext> mockHttpContext = new(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            ActionResult result = await controller.AddFollowAsync(UserName, model);

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

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());

            Dictionary<string, object> mockDatabaseCommandParameters = new() { { "@UserId", UserId }, { "@FollowUserId", FollowUserId } };
            Mock<IDatabaseCommand> mockDatabaseCommand = MockDatabaseHelper.CreateMockDatabaseCommand(mockDatabaseCommandParameters, 1);

            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            mockDatabaseCommandFactory
                .Setup(_ => _.Create())
                .Returns(mockDatabaseCommand.Object);

            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);
            ApplicationUser mockUser = new();
            ApplicationUser mockFollowUser = new();
            Mock<ClaimsPrincipal> mockCurrentUser = new(MockBehavior.Strict);
            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
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

            Mock<HttpContext> mockHttpContext = new(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            ActionResult result = await controller.RemoveFollowAsync(UserName, FollowUserName);

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

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);
            Mock<ClaimsPrincipal> mockCurrentUser = new(MockBehavior.Strict);
            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult<ApplicationUser>(null));

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            ActionResult result = await controller.RemoveFollowAsync(UserName, FollowUserName);

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

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);
            ApplicationUser mockUser = new();
            Mock<ClaimsPrincipal> mockCurrentUser = new(MockBehavior.Strict);
            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult(mockUser));
            mockUserManager
                .Setup(_ => _.GetUserIdAsync(mockUser))
                .Returns(Task.FromResult<string>(null));

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            ActionResult result = await controller.RemoveFollowAsync(UserName, FollowUserName);

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

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);
            ApplicationUser mockUser = new();
            Mock<ClaimsPrincipal> mockCurrentUser = new(MockBehavior.Strict);
            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
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

            Mock<HttpContext> mockHttpContext = new(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };
            ActionResult result = await controller.RemoveFollowAsync(UserName, FollowUserName);

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

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);
            ApplicationUser mockUser = new();
            ApplicationUser mockFollowUser = new();
            Mock<ClaimsPrincipal> mockCurrentUser = new(MockBehavior.Strict);
            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
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

            Mock<HttpContext> mockHttpContext = new(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            ActionResult result = await controller.RemoveFollowAsync(UserName, FollowUserName);

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

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);
            ApplicationUser mockUser = new();
            Mock<ClaimsPrincipal> mockCurrentUser = new(MockBehavior.Strict);
            mockCurrentUser
                .Setup(_ => _.IsInRole("Admin"))
                .Returns(false);

            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult(mockUser));
            mockUserManager
                .Setup(_ => _.GetUserIdAsync(mockUser))
                .Returns(Task.FromResult(UserId));
            mockUserManager
                .Setup(_ => _.GetUserId(mockCurrentUser.Object))
                .Returns("SomeOtherUserId");

            Mock<HttpContext> mockHttpContext = new(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            ActionResult result = await controller.RemoveFollowAsync(UserName, FollowUserName);

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

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());

            Dictionary<string, object> mockDatabaseCommandParameters = new() { { "@UserId", UserId }, { "@FollowUserId", FollowUserId } };
            Mock<IDatabaseCommand> mockDatabaseCommand = MockDatabaseHelper.CreateMockDatabaseCommand(mockDatabaseCommandParameters, 1);

            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            mockDatabaseCommandFactory
                .Setup(_ => _.Create())
                .Returns(mockDatabaseCommand.Object);

            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);
            ApplicationUser mockUser = new();
            ApplicationUser mockFollowUser = new();
            Mock<ClaimsPrincipal> mockCurrentUser = new(MockBehavior.Strict);
            mockCurrentUser
                .Setup(_ => _.IsInRole("Admin"))
                .Returns(true);

            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
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

            Mock<HttpContext> mockHttpContext = new(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            ActionResult result = await controller.RemoveFollowAsync(UserName, FollowUserName);

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

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());

            Dictionary<string, object> mockDatabaseCommandParameters = new() { { "@UserId", UserId }, { "@FollowUserId", FollowUserId } };
            Mock<IDatabaseCommand> mockDatabaseCommand = MockDatabaseHelper.CreateMockDatabaseCommand(mockDatabaseCommandParameters, 0);

            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            mockDatabaseCommandFactory
                .Setup(_ => _.Create())
                .Returns(mockDatabaseCommand.Object);

            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);
            ApplicationUser mockUser = new();
            ApplicationUser mockFollowUser = new();
            Mock<ClaimsPrincipal> mockCurrentUser = new(MockBehavior.Strict);
            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
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

            Mock<HttpContext> mockHttpContext = new(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            ActionResult result = await controller.RemoveFollowAsync(UserName, FollowUserName);

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

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);

            UserSettings userSettings = new();
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);
            mockUserSettingsProvider
                .Setup(_ => _.GetAsync(UserId))
                .Returns(Task.FromResult(userSettings));

            ApplicationUser mockUser = new();
            Mock<ClaimsPrincipal> mockCurrentUser = new(MockBehavior.Strict);
            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult(mockUser));
            mockUserManager
                .Setup(_ => _.GetUserIdAsync(mockUser))
                .Returns(Task.FromResult(UserId));
            mockUserManager
                .Setup(_ => _.GetUserId(mockCurrentUser.Object))
                .Returns(UserId);

            Mock<HttpContext> mockHttpContext = new(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            ActionResult<UserSettings> result = await controller.GetSettingsAsync(UserName);
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

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);
            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult<ApplicationUser>(null));

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);

            ActionResult<UserSettings> result = await controller.GetSettingsAsync(UserName);

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

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);

            ApplicationUser mockUser = new();
            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult(mockUser));
            mockUserManager
                .Setup(_ => _.GetUserIdAsync(mockUser))
                .Returns(Task.FromResult<string>(null));

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);

            ActionResult<UserSettings> result = await controller.GetSettingsAsync(UserName);

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

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);

            ApplicationUser mockUser = new();
            Mock<ClaimsPrincipal> mockCurrentUser = new(MockBehavior.Strict);
            mockCurrentUser
                .Setup(_ => _.IsInRole("Admin"))
                .Returns(false);
            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult(mockUser));
            mockUserManager
                .Setup(_ => _.GetUserIdAsync(mockUser))
                .Returns(Task.FromResult(UserId));
            mockUserManager
                .Setup(_ => _.GetUserId(mockCurrentUser.Object))
                .Returns("SomeOtherUserId");

            Mock<HttpContext> mockHttpContext = new(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            ActionResult<UserSettings> result = await controller.GetSettingsAsync(UserName);

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

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);

            UserSettings userSettings = new();
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);
            mockUserSettingsProvider
                .Setup(_ => _.GetAsync(UserId))
                .Returns(Task.FromResult(userSettings));

            ApplicationUser mockUser = new();
            Mock<ClaimsPrincipal> mockCurrentUser = new(MockBehavior.Strict);
            mockCurrentUser
                .Setup(_ => _.IsInRole("Admin"))
                .Returns(true);
            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult(mockUser));
            mockUserManager
                .Setup(_ => _.GetUserIdAsync(mockUser))
                .Returns(Task.FromResult(UserId));
            mockUserManager
                .Setup(_ => _.GetUserId(mockCurrentUser.Object))
                .Returns("SomeOtherUserId");

            Mock<HttpContext> mockHttpContext = new(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            ActionResult<UserSettings> result = await controller.GetSettingsAsync(UserName);
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

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            UserSettings userSettings = new();
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);
            mockUserSettingsProvider
                .Setup(_ => _.PatchAsync(UserId, userSettings))
                .Returns(Task.CompletedTask);

            ApplicationUser mockUser = new();
            Mock<ClaimsPrincipal> mockCurrentUser = new(MockBehavior.Strict);
            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult(mockUser));
            mockUserManager
                .Setup(_ => _.GetUserIdAsync(mockUser))
                .Returns(Task.FromResult(UserId));
            mockUserManager
                .Setup(_ => _.GetUserId(mockCurrentUser.Object))
                .Returns(UserId);

            Mock<HttpContext> mockHttpContext = new(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            ActionResult result = await controller.PatchSettingsAsync(UserName, userSettings);

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

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            UserSettings userSettings = new();
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);
            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult<ApplicationUser>(null));

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);

            ActionResult result = await controller.PatchSettingsAsync(UserName, userSettings);

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

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            UserSettings userSettings = new();
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);

            ApplicationUser mockUser = new();
            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult(mockUser));
            mockUserManager
                .Setup(_ => _.GetUserIdAsync(mockUser))
                .Returns(Task.FromResult<string>(null));

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);

            ActionResult result = await controller.PatchSettingsAsync(UserName, userSettings);

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

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            UserSettings userSettings = new();
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);

            ApplicationUser mockUser = new();
            Mock<ClaimsPrincipal> mockCurrentUser = new(MockBehavior.Strict);
            mockCurrentUser
                .Setup(_ => _.IsInRole("Admin"))
                .Returns(false);
            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult(mockUser));
            mockUserManager
                .Setup(_ => _.GetUserIdAsync(mockUser))
                .Returns(Task.FromResult(UserId));
            mockUserManager
                .Setup(_ => _.GetUserId(mockCurrentUser.Object))
                .Returns("SomeOtherUserId");

            Mock<HttpContext> mockHttpContext = new(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            ActionResult result = await controller.PatchSettingsAsync(UserName, userSettings);

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

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);
            ApplicationUser mockUser = new();
            Mock<ClaimsPrincipal> mockCurrentUser = new(MockBehavior.Strict);
            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
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

            List<ExternalLogin> logins = new()
            {
                new ExternalLogin { ProviderName = "SomeProvider0", ExternalUserId = "SomeExternalUserId0" },
                new ExternalLogin { ProviderName = "SomeProvider1", ExternalUserId = "SomeExternalUserId1" },
                new ExternalLogin { ProviderName = "SomeProvider2", ExternalUserId = "SomeExternalUserId2" },
            };
            mockUserManager
                .Setup(_ => _.GetLoginsAsync(mockUser))
                .Returns(Task.FromResult<IList<UserLoginInfo>>(logins.Select(login => new UserLoginInfo(login.ProviderName, login.ExternalUserId, login.ProviderName)).ToList()));

            Mock<HttpContext> mockHttpContext = new(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            ActionResult<UserLogins> result = await controller.GetLoginsAsync(UserName);
            Assert.NotNull(result);

            UserLogins model = result.Value;
            Assert.True(model.HasPassword);

            Assert.Equal(logins.Count, model.ExternalLogins.Count);
            for (int i = 0; i < model.ExternalLogins.Count; i++)
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

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);
            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult<ApplicationUser>(null));

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);

            ActionResult<UserLogins> result = await controller.GetLoginsAsync(UserName);

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

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);

            ApplicationUser mockUser = new();
            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult(mockUser));
            mockUserManager
                .Setup(_ => _.GetUserIdAsync(mockUser))
                .Returns(Task.FromResult<string>(null));

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);

            ActionResult<UserLogins> result = await controller.GetLoginsAsync(UserName);

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

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);

            ApplicationUser mockUser = new();
            Mock<ClaimsPrincipal> mockCurrentUser = new(MockBehavior.Strict);
            mockCurrentUser
                .Setup(_ => _.IsInRole("Admin"))
                .Returns(false);
            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult(mockUser));
            mockUserManager
                .Setup(_ => _.GetUserIdAsync(mockUser))
                .Returns(Task.FromResult(UserId));
            mockUserManager
                .Setup(_ => _.GetUserId(mockCurrentUser.Object))
                .Returns("SomeOtherUserId");

            Mock<HttpContext> mockHttpContext = new(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            ActionResult<UserLogins> result = await controller.GetLoginsAsync(UserName);

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

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);
            ApplicationUser mockUser = new();
            Mock<ClaimsPrincipal> mockCurrentUser = new(MockBehavior.Strict);
            mockCurrentUser
                .Setup(_ => _.IsInRole("Admin"))
                .Returns(true);
            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
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

            List<ExternalLogin> logins = new()
            {
                new ExternalLogin { ProviderName = "SomeProvider0", ExternalUserId = "SomeExternalUserId0" },
                new ExternalLogin { ProviderName = "SomeProvider1", ExternalUserId = "SomeExternalUserId1" },
                new ExternalLogin { ProviderName = "SomeProvider2", ExternalUserId = "SomeExternalUserId2" },
            };
            mockUserManager
                .Setup(_ => _.GetLoginsAsync(mockUser))
                .Returns(Task.FromResult<IList<UserLoginInfo>>(logins.Select(login => new UserLoginInfo(login.ProviderName, login.ExternalUserId, login.ProviderName)).ToList()));

            Mock<HttpContext> mockHttpContext = new(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            ActionResult<UserLogins> result = await controller.GetLoginsAsync(UserName);
            Assert.NotNull(result);

            UserLogins model = result.Value;
            Assert.True(model.HasPassword);

            Assert.Equal(logins.Count, model.ExternalLogins.Count);
            for (int i = 0; i < model.ExternalLogins.Count; i++)
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

            ExternalLogin model = new()
            {
                ProviderName = Provider,
                ExternalUserId = ExternalUserId,
            };

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);
            ApplicationUser mockUser = new();
            Mock<ClaimsPrincipal> mockCurrentUser = new(MockBehavior.Strict);
            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
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

            Mock<HttpContext> mockHttpContext = new(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            ActionResult result = await controller.RemoveLoginAsync(UserName, model);

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

            ExternalLogin model = new()
            {
                ProviderName = Provider,
                ExternalUserId = ExternalUserId,
            };

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);
            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult<ApplicationUser>(null));

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);

            ActionResult result = await controller.RemoveLoginAsync(UserName, model);

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

            ExternalLogin model = new()
            {
                ProviderName = Provider,
                ExternalUserId = ExternalUserId,
            };

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);

            ApplicationUser mockUser = new();
            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult(mockUser));
            mockUserManager
                .Setup(_ => _.GetUserIdAsync(mockUser))
                .Returns(Task.FromResult<string>(null));

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);

            ActionResult result = await controller.RemoveLoginAsync(UserName, model);

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

            ExternalLogin model = new()
            {
                ProviderName = Provider,
                ExternalUserId = ExternalUserId,
            };

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);

            ApplicationUser mockUser = new();
            Mock<ClaimsPrincipal> mockCurrentUser = new(MockBehavior.Strict);
            mockCurrentUser
                .Setup(_ => _.IsInRole("Admin"))
                .Returns(false);
            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult(mockUser));
            mockUserManager
                .Setup(_ => _.GetUserIdAsync(mockUser))
                .Returns(Task.FromResult(UserId));
            mockUserManager
                .Setup(_ => _.GetUserId(mockCurrentUser.Object))
                .Returns("SomeOtherUserId");

            Mock<HttpContext> mockHttpContext = new(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            ActionResult result = await controller.RemoveLoginAsync(UserName, model);

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

            ExternalLogin model = new()
            {
                ProviderName = Provider,
                ExternalUserId = ExternalUserId,
            };

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);

            ApplicationUser mockUser = new();
            Mock<ClaimsPrincipal> mockCurrentUser = new(MockBehavior.Strict);
            mockCurrentUser
                .Setup(_ => _.IsInRole("Admin"))
                .Returns(true);
            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
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

            Mock<HttpContext> mockHttpContext = new(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            ActionResult result = await controller.RemoveLoginAsync(UserName, model);

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

            ExternalLogin model = new()
            {
                ProviderName = Provider,
                ExternalUserId = ExternalUserId,
            };

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);
            ApplicationUser mockUser = new();
            Mock<ClaimsPrincipal> mockCurrentUser = new(MockBehavior.Strict);
            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
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

            Mock<HttpContext> mockHttpContext = new(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            ActionResult result = await controller.RemoveLoginAsync(UserName, model);

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

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            UserSettings userSettings = new();
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);
            mockUserSettingsProvider
                .Setup(_ => _.PatchAsync(UserId, userSettings))
                .Returns(Task.CompletedTask);

            ApplicationUser mockUser = new();
            Mock<ClaimsPrincipal> mockCurrentUser = new(MockBehavior.Strict);
            mockCurrentUser
                .Setup(_ => _.IsInRole("Admin"))
                .Returns(true);
            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult(mockUser));
            mockUserManager
                .Setup(_ => _.GetUserIdAsync(mockUser))
                .Returns(Task.FromResult(UserId));
            mockUserManager
                .Setup(_ => _.GetUserId(mockCurrentUser.Object))
                .Returns("SomeOtherUserId");

            Mock<HttpContext> mockHttpContext = new(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            ActionResult result = await controller.PatchSettingsAsync(UserName, userSettings);

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

            SetPasswordRequest model = new()
            {
                NewPassword = NewPassword,
            };

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);
            ApplicationUser mockUser = new();
            Mock<ClaimsPrincipal> mockCurrentUser = new(MockBehavior.Strict);
            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
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

            Mock<HttpContext> mockHttpContext = new(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            ActionResult result = await controller.SetPasswordAsync(UserName, model);

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

            SetPasswordRequest model = new()
            {
                NewPassword = NewPassword,
            };

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);
            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult<ApplicationUser>(null));

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);

            ActionResult result = await controller.SetPasswordAsync(UserName, model);

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

            SetPasswordRequest model = new()
            {
                NewPassword = NewPassword,
            };

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);

            ApplicationUser mockUser = new();
            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult(mockUser));
            mockUserManager
                .Setup(_ => _.GetUserIdAsync(mockUser))
                .Returns(Task.FromResult<string>(null));

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);

            ActionResult result = await controller.SetPasswordAsync(UserName, model);

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

            SetPasswordRequest model = new()
            {
                NewPassword = NewPassword,
            };

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);

            ApplicationUser mockUser = new();
            Mock<ClaimsPrincipal> mockCurrentUser = new(MockBehavior.Strict);
            mockCurrentUser
                .Setup(_ => _.IsInRole("Admin"))
                .Returns(false);
            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult(mockUser));
            mockUserManager
                .Setup(_ => _.GetUserIdAsync(mockUser))
                .Returns(Task.FromResult(UserId));
            mockUserManager
                .Setup(_ => _.GetUserId(mockCurrentUser.Object))
                .Returns("SomeOtherUserId");

            Mock<HttpContext> mockHttpContext = new(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            ActionResult result = await controller.SetPasswordAsync(UserName, model);

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

            SetPasswordRequest model = new()
            {
                NewPassword = NewPassword,
            };

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);

            ApplicationUser mockUser = new();
            Mock<ClaimsPrincipal> mockCurrentUser = new(MockBehavior.Strict);
            mockCurrentUser
                .Setup(_ => _.IsInRole("Admin"))
                .Returns(true);
            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
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

            Mock<HttpContext> mockHttpContext = new(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            ActionResult result = await controller.SetPasswordAsync(UserName, model);

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

            SetPasswordRequest model = new()
            {
                NewPassword = NewPassword,
            };

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);
            ApplicationUser mockUser = new();
            Mock<ClaimsPrincipal> mockCurrentUser = new(MockBehavior.Strict);
            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
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

            Mock<HttpContext> mockHttpContext = new(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            ActionResult result = await controller.SetPasswordAsync(UserName, model);

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

            ChangePasswordRequest model = new()
            {
                CurrentPassword = CurrentPassword,
                NewPassword = NewPassword,
            };

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);
            ApplicationUser mockUser = new();
            Mock<ClaimsPrincipal> mockCurrentUser = new(MockBehavior.Strict);
            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
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

            Mock<HttpContext> mockHttpContext = new(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            ActionResult result = await controller.ChangePasswordAsync(UserName, model);

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

            ChangePasswordRequest model = new()
            {
                CurrentPassword = CurrentPassword,
                NewPassword = NewPassword,
            };

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);
            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult<ApplicationUser>(null));

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);

            ActionResult result = await controller.ChangePasswordAsync(UserName, model);

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

            ChangePasswordRequest model = new()
            {
                CurrentPassword = CurrentPassword,
                NewPassword = NewPassword,
            };

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);

            ApplicationUser mockUser = new();
            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult(mockUser));
            mockUserManager
                .Setup(_ => _.GetUserIdAsync(mockUser))
                .Returns(Task.FromResult<string>(null));

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);

            ActionResult result = await controller.ChangePasswordAsync(UserName, model);

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

            ChangePasswordRequest model = new()
            {
                CurrentPassword = CurrentPassword,
                NewPassword = NewPassword,
            };

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);

            ApplicationUser mockUser = new();
            Mock<ClaimsPrincipal> mockCurrentUser = new(MockBehavior.Strict);
            mockCurrentUser
                .Setup(_ => _.IsInRole("Admin"))
                .Returns(false);
            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByNameAsync(UserName))
                .Returns(Task.FromResult(mockUser));
            mockUserManager
                .Setup(_ => _.GetUserIdAsync(mockUser))
                .Returns(Task.FromResult(UserId));
            mockUserManager
                .Setup(_ => _.GetUserId(mockCurrentUser.Object))
                .Returns("SomeOtherUserId");

            Mock<HttpContext> mockHttpContext = new(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            ActionResult result = await controller.ChangePasswordAsync(UserName, model);

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

            ChangePasswordRequest model = new()
            {
                CurrentPassword = CurrentPassword,
                NewPassword = NewPassword,
            };

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);

            ApplicationUser mockUser = new();
            Mock<ClaimsPrincipal> mockCurrentUser = new(MockBehavior.Strict);
            mockCurrentUser
                .Setup(_ => _.IsInRole("Admin"))
                .Returns(true);
            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
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

            Mock<HttpContext> mockHttpContext = new(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            ActionResult result = await controller.ChangePasswordAsync(UserName, model);

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

            ChangePasswordRequest model = new()
            {
                CurrentPassword = CurrentPassword,
                NewPassword = NewPassword,
            };

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);
            ApplicationUser mockUser = new();
            Mock<ClaimsPrincipal> mockCurrentUser = new(MockBehavior.Strict);
            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
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

            Mock<HttpContext> mockHttpContext = new(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockCurrentUser.Object).Verifiable();

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            ActionResult result = await controller.ChangePasswordAsync(UserName, model);

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

            ResetPasswordRequest model = new()
            {
                Email = "SomeEmail",
            };

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);

            ApplicationUser mockUser = new();
            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByEmailAsync(model.Email))
                .Returns(Task.FromResult(mockUser));
            mockUserManager
                .Setup(_ => _.GeneratePasswordResetTokenAsync(mockUser))
                .Returns(Task.FromResult(Code));

            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            mockEmailSender
                .Setup(_ => _.SendEmailAsync(model.Email, It.IsAny<string>(), It.Is<string>(str => str.Contains(Code, StringComparison.Ordinal))))
                .Returns(Task.CompletedTask);

            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            ActionResult result = await controller.ResetPasswordAsync(model);

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
            ResetPasswordRequest model = new()
            {
                Email = "SomeEmail",
            };

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);
            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByEmailAsync(model.Email))
                .Returns(Task.FromResult((ApplicationUser)null));

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            ActionResult result = await controller.ResetPasswordAsync(model);

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
            ResetPasswordConfirmationRequest model = new()
            {
                Email = "SomeEmail",
                Password = "SomePassword",
                Code = "SomeCode",
            };

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);
            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            ApplicationUser mockUser = new();
            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByEmailAsync(model.Email))
                .Returns(Task.FromResult(mockUser));
            mockUserManager
                .Setup(_ => _.ResetPasswordAsync(mockUser, model.Code, model.Password))
                .Returns(Task.FromResult(IdentityResult.Success));

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            ActionResult result = await controller.ResetPasswordConfirmationAsync(model);

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
            ResetPasswordConfirmationRequest model = new()
            {
                Email = "SomeEmail",
                Password = "SomePassword",
                Code = "SomeCode",
            };

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);
            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByEmailAsync(model.Email))
                .Returns(Task.FromResult((ApplicationUser)null));

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            ActionResult result = await controller.ResetPasswordConfirmationAsync(model);

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
            ResetPasswordConfirmationRequest model = new()
            {
                Email = "SomeEmail",
                Password = "SomePassword",
                Code = "SomeCode",
            };

            GameData gameData = MockGameData.RealData;
            TelemetryClient telemetryClient = new(new TelemetryConfiguration());
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            Mock<IUserSettingsProvider> mockUserSettingsProvider = new(MockBehavior.Strict);
            Mock<IEmailSender> mockEmailSender = new(MockBehavior.Strict);
            Mock<IClanManager> mockClanManager = new(MockBehavior.Strict);

            ApplicationUser mockUser = new();
            Mock<UserManager<ApplicationUser>> mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.FindByEmailAsync(model.Email))
                .Returns(Task.FromResult(mockUser));
            mockUserManager
                .Setup(_ => _.ResetPasswordAsync(mockUser, model.Code, model.Password))
                .Returns(Task.FromResult(IdentityResult.Failed(new IdentityError { Description = "SomeDescription" })));

            UserController controller = new(
                gameData,
                telemetryClient,
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                mockUserManager.Object,
                mockEmailSender.Object,
                mockClanManager.Object);
            ActionResult result = await controller.ResetPasswordConfirmationAsync(model);

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