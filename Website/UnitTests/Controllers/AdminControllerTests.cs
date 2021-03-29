// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClickerHeroesTrackerWebsite.Services.Database;
using ClickerHeroesTrackerWebsite.Tests.Mocks;
using Moq;
using Website.Controllers;
using Website.Models.Api.Admin;
using Xunit;

namespace UnitTests.Controllers
{
    public static class AdminControllerTests
    {
        [Fact]
        public static async Task StaleUploads_Success()
        {
            List<int> expectedIds = Enumerable.Range(0, 3).ToList();
            List<IDictionary<string, object>> datasets = expectedIds
                .Select<int, IDictionary<string, object>>(id => new Dictionary<string, object> { { "Id", id } })
                .ToList();
            Mock<System.Data.IDataReader> mockDataReader = MockDatabaseHelper.CreateMockDataReader(datasets);
            Mock<IDatabaseCommand> mockDatabaseCommand = MockDatabaseHelper.CreateMockDatabaseCommand(new Dictionary<string, object>(), mockDataReader.Object);

            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            mockDatabaseCommandFactory.Setup(_ => _.Create()).Returns(mockDatabaseCommand.Object).Verifiable();

            AdminController controller = new(mockDatabaseCommandFactory.Object);
            Microsoft.AspNetCore.Mvc.ActionResult<List<int>> result = await controller.StaleUploadsAsync();

            Assert.NotNull(result);
            List<int> actualIds = result.Value;

            Assert.Equal(expectedIds.Count, actualIds.Count);
            for (int i = 0; i < actualIds.Count; i++)
            {
                Assert.Equal(expectedIds[i], actualIds[i]);
            }

            mockDatabaseCommandFactory.VerifyAll();
        }

        [Fact]
        public static async Task ListAuthTokens_Success()
        {
            const int NumInvalidTokens = 100;
            Mock<IDatabaseCommand> mockDatabaseCommand = MockDatabaseHelper.CreateMockDatabaseCommand(new Dictionary<string, object>(), NumInvalidTokens);

            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            mockDatabaseCommandFactory.Setup(_ => _.Create()).Returns(mockDatabaseCommand.Object).Verifiable();

            AdminController controller = new(mockDatabaseCommandFactory.Object);
            Microsoft.AspNetCore.Mvc.ActionResult<int> result = await controller.CountInvalidAuthTokensAsync();

            Assert.NotNull(result);
            int actualCount = result.Value;

            Assert.Equal(NumInvalidTokens, actualCount);

            mockDatabaseCommandFactory.VerifyAll();
        }

        [Fact]
        public static async Task PruneInvalidAuthTokens_Success()
        {
            PruneInvalidAuthTokensRequest model = new()
            {
                BatchSize = 100,
            };

            Mock<IDatabaseCommand> mockDatabaseCommand = MockDatabaseHelper.CreateMockDatabaseCommand(new Dictionary<string, object> { { "BatchSize", model.BatchSize } });

            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            mockDatabaseCommandFactory.Setup(_ => _.Create()).Returns(mockDatabaseCommand.Object).Verifiable();

            AdminController controller = new(mockDatabaseCommandFactory.Object);
            await controller.PruneInvalidAuthTokensAsync(model);

            mockDatabaseCommandFactory.VerifyAll();
        }
    }
}
