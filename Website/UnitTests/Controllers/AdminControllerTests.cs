﻿// <copyright file="AdminControllerTests.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace UnitTests.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using ClickerHeroesTrackerWebsite.Services.Database;
    using ClickerHeroesTrackerWebsite.Services.UploadProcessing;
    using ClickerHeroesTrackerWebsite.Tests.Mocks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using Website.Controllers.Api;
    using Website.Models.Api.Admin;
    using Website.Services.UploadProcessing;
    using Xunit;

    public class AdminControllerTests
    {
        [Fact]
        public async Task Queues_Success()
        {
            var priorities = Enum.GetValues(typeof(UploadProcessingMessagePriority)) as UploadProcessingMessagePriority[];
            var expectedQueues = Enumerable.Range(0, priorities.Length * 3)
                .Select(i => new UploadQueueStats { Priority = priorities[i % priorities.Length], NumMessages = i })
                .ToList();

            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var mockUploadScheduler = new Mock<IUploadScheduler>(MockBehavior.Strict);
            mockUploadScheduler
                .Setup(_ => _.RetrieveQueueStatsAsync())
                .Returns(Task.FromResult<IEnumerable<UploadQueueStats>>(expectedQueues));
            var mockUserManager = MockUserManager.CreateMock();

            var controller = new AdminController(
                mockDatabaseCommandFactory.Object,
                mockUploadScheduler.Object,
                mockUserManager.Object);
            var result = await controller.Queues();

            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedQueues, ((OkObjectResult)result).Value);

            mockDatabaseCommandFactory.VerifyAll();
            mockUploadScheduler.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public async Task Recompute_Success()
        {
            const string UserId = "SomeUserId";
            var model = new RecomputeRequest
            {
                UploadIds = new[] { 0, 1, 2 },
                Priority = UploadProcessingMessagePriority.High,
            };

            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);

            List<UploadProcessingMessage> messages = null;
            var mockUploadScheduler = new Mock<IUploadScheduler>(MockBehavior.Strict);
            mockUploadScheduler
                .Setup(_ => _.ScheduleAsync(It.IsAny<IEnumerable<UploadProcessingMessage>>()))
                .Callback<IEnumerable<UploadProcessingMessage>>(_ => messages = _.ToList())
                .Returns(Task.CompletedTask);

            var mockUser = new Mock<ClaimsPrincipal>(MockBehavior.Strict);
            var mockUserManager = MockUserManager.CreateMock();
            mockUserManager
                .Setup(_ => _.GetUserId(mockUser.Object))
                .Returns(UserId);

            var mockHttpContext = new Mock<HttpContext>(MockBehavior.Strict);
            mockHttpContext.SetupGet(_ => _.User).Returns(mockUser.Object);

            var controller = new AdminController(
                mockDatabaseCommandFactory.Object,
                mockUploadScheduler.Object,
                mockUserManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            var result = await controller.Recompute(model);

            Assert.NotNull(result);
            Assert.IsType<OkResult>(result);

            Assert.Equal(model.UploadIds.Count, messages.Count);
            for (var i = 0; i < messages.Count; i++)
            {
                Assert.Equal(model.UploadIds[i], messages[i].UploadId);
                Assert.Equal(model.Priority, messages[i].Priority);
                Assert.Equal(UserId, messages[i].Requester);
            }

            mockDatabaseCommandFactory.VerifyAll();
            mockUploadScheduler.VerifyAll();
            mockUser.VerifyAll();
            mockHttpContext.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public async Task Recompute_InvalidModelState()
        {
            var model = new RecomputeRequest
            {
                UploadIds = new[] { 1, 2, 3 },
                Priority = UploadProcessingMessagePriority.High,
            };

            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var mockUploadScheduler = new Mock<IUploadScheduler>(MockBehavior.Strict);
            var mockUser = new Mock<ClaimsPrincipal>(MockBehavior.Strict);
            var mockUserManager = MockUserManager.CreateMock();
            var mockHttpContext = new Mock<HttpContext>(MockBehavior.Strict);

            var controller = new AdminController(
                mockDatabaseCommandFactory.Object,
                mockUploadScheduler.Object,
                mockUserManager.Object);
            controller.ModelState.AddModelError("SomeKey", "SomeErrorMessage");

            var result = await controller.Recompute(model);

            Assert.NotNull(result);
            Assert.IsType<BadRequestObjectResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUploadScheduler.VerifyAll();
            mockUser.VerifyAll();
            mockHttpContext.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public async Task Recompute_NoUploadIds()
        {
            var model = new RecomputeRequest
            {
                UploadIds = Array.Empty<int>(),
                Priority = UploadProcessingMessagePriority.High,
            };

            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var mockUploadScheduler = new Mock<IUploadScheduler>(MockBehavior.Strict);
            var mockUser = new Mock<ClaimsPrincipal>(MockBehavior.Strict);
            var mockUserManager = MockUserManager.CreateMock();
            var mockHttpContext = new Mock<HttpContext>(MockBehavior.Strict);

            var controller = new AdminController(
                mockDatabaseCommandFactory.Object,
                mockUploadScheduler.Object,
                mockUserManager.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            var result = await controller.Recompute(model);

            Assert.NotNull(result);
            Assert.IsType<BadRequestResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUploadScheduler.VerifyAll();
            mockUser.VerifyAll();
            mockHttpContext.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public async Task ClearQueue_Success()
        {
            const int NumMessages = 123;
            var model = new ClearQueueRequest
            {
                Priority = UploadProcessingMessagePriority.High,
            };

            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var mockUploadScheduler = new Mock<IUploadScheduler>(MockBehavior.Strict);
            mockUploadScheduler
                .Setup(_ => _.ClearQueueAsync(model.Priority))
                .Returns(Task.FromResult(NumMessages));
            var mockUserManager = MockUserManager.CreateMock();

            var controller = new AdminController(
                mockDatabaseCommandFactory.Object,
                mockUploadScheduler.Object,
                mockUserManager.Object);
            var result = await controller.ClearQueue(model);

            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(NumMessages, ((OkObjectResult)result).Value);

            mockDatabaseCommandFactory.VerifyAll();
            mockUploadScheduler.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public async Task ClearQueue_InvalidModelState()
        {
            var model = new ClearQueueRequest
            {
                Priority = UploadProcessingMessagePriority.High,
            };

            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var mockUploadScheduler = new Mock<IUploadScheduler>(MockBehavior.Strict);
            var mockUserManager = MockUserManager.CreateMock();

            var controller = new AdminController(
                mockDatabaseCommandFactory.Object,
                mockUploadScheduler.Object,
                mockUserManager.Object);
            controller.ModelState.AddModelError("SomeKey", "SomeErrorMessage");

            var result = await controller.ClearQueue(model);

            Assert.NotNull(result);
            Assert.IsType<BadRequestObjectResult>(result);

            mockDatabaseCommandFactory.VerifyAll();
            mockUploadScheduler.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }

        [Fact]
        public void StaleUploads_Success()
        {
            var expectedIds = Enumerable.Range(0, 3).ToList();
            var datasets = expectedIds
                .Select<int, IDictionary<string, object>>(id => new Dictionary<string, object> { { "Id", id } })
                .ToList();
            var mockDataReader = MockDatabaseHelper.CreateMockDataReader(datasets);
            var mockDatabaseCommand = MockDatabaseHelper.CreateMockDatabaseCommand(new Dictionary<string, object>(), mockDataReader.Object);

            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            mockDatabaseCommandFactory.Setup(_ => _.Create()).Returns(mockDatabaseCommand.Object).Verifiable();

            var mockUploadScheduler = new Mock<IUploadScheduler>(MockBehavior.Strict);
            var mockUserManager = MockUserManager.CreateMock();

            var controller = new AdminController(
                mockDatabaseCommandFactory.Object,
                mockUploadScheduler.Object,
                mockUserManager.Object);
            var result = controller.StaleUploads();

            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);

            var actualIds = ((OkObjectResult)result).Value as List<int>;

            Assert.Equal(expectedIds.Count, actualIds.Count);
            for (var i = 0; i < actualIds.Count; i++)
            {
                Assert.Equal(expectedIds[i], actualIds[i]);
            }

            mockDatabaseCommandFactory.VerifyAll();
            mockUploadScheduler.VerifyAll();

            // Workaround for a Moq bug. See: https://github.com/moq/moq4/issues/456#issuecomment-331692858
            mockUserManager.Object.Logger = mockUserManager.Object.Logger;
            mockUserManager.VerifyAll();
        }
    }
}