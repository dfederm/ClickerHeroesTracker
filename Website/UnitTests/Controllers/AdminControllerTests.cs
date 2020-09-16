// <copyright file="AdminControllerTests.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace UnitTests.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using ClickerHeroesTrackerWebsite.Services.Database;
    using ClickerHeroesTrackerWebsite.Tests.Mocks;
    using Moq;
    using Website.Controllers;
    using Website.Models.Api.Admin;
    using Xunit;

    public static class AdminControllerTests
    {
        [Fact]
        public static async Task StaleUploads_Success()
        {
            var expectedIds = Enumerable.Range(0, 3).ToList();
            var datasets = expectedIds
                .Select<int, IDictionary<string, object>>(id => new Dictionary<string, object> { { "Id", id } })
                .ToList();
            var mockDataReader = MockDatabaseHelper.CreateMockDataReader(datasets);
            var mockDatabaseCommand = MockDatabaseHelper.CreateMockDatabaseCommand(new Dictionary<string, object>(), mockDataReader.Object);

            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            mockDatabaseCommandFactory.Setup(_ => _.Create()).Returns(mockDatabaseCommand.Object).Verifiable();

            var controller = new AdminController(mockDatabaseCommandFactory.Object);
            var result = await controller.StaleUploads();

            Assert.NotNull(result);
            var actualIds = result.Value;

            Assert.Equal(expectedIds.Count, actualIds.Count);
            for (var i = 0; i < actualIds.Count; i++)
            {
                Assert.Equal(expectedIds[i], actualIds[i]);
            }

            mockDatabaseCommandFactory.VerifyAll();
        }

        [Fact]
        public static async Task ListAuthTokens_Success()
        {
            const int NumInvalidTokens = 100;
            var mockDatabaseCommand = MockDatabaseHelper.CreateMockDatabaseCommand(new Dictionary<string, object>(), NumInvalidTokens);

            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            mockDatabaseCommandFactory.Setup(_ => _.Create()).Returns(mockDatabaseCommand.Object).Verifiable();

            var controller = new AdminController(mockDatabaseCommandFactory.Object);
            var result = await controller.CountInvalidAuthTokens();

            Assert.NotNull(result);
            var actualCount = result.Value;

            Assert.Equal(NumInvalidTokens, actualCount);

            mockDatabaseCommandFactory.VerifyAll();
        }

        [Fact]
        public static async Task PruneInvalidAuthTokens_Success()
        {
            var model = new PruneInvalidAuthTokensRequest
            {
                BatchSize = 100,
            };

            var mockDatabaseCommand = MockDatabaseHelper.CreateMockDatabaseCommand(new Dictionary<string, object> { { "BatchSize", model.BatchSize } });

            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            mockDatabaseCommandFactory.Setup(_ => _.Create()).Returns(mockDatabaseCommand.Object).Verifiable();

            var controller = new AdminController(mockDatabaseCommandFactory.Object);
            await controller.PruneInvalidAuthTokens(model);

            mockDatabaseCommandFactory.VerifyAll();
        }
    }
}
