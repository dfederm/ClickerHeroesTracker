// <copyright file="VersionControllerTests.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Tests.Controllers
{
    using System.Collections.Generic;
    using ClickerHeroesTrackerWebsite.Configuration;
    using ClickerHeroesTrackerWebsite.Controllers;
    using Microsoft.AspNetCore.Hosting;
    using Moq;
    using Xunit;

    public static class VersionControllerTests
    {
        [Fact]
        public static void Version()
        {
            var mockBuildInfoProvider = new Mock<IBuildInfoProvider>(MockBehavior.Strict);
            mockBuildInfoProvider.SetupGet(_ => _.Changelist).Returns("SomeChangelist").Verifiable();
            mockBuildInfoProvider.SetupGet(_ => _.BuildUrl).Returns("SomeBuildUrl").Verifiable();

            var webclient = new Dictionary<string, string>
            {
                { "bundle0", "version0" },
                { "bundle1", "version1" },
                { "bundle2", "version2" },
            };
            mockBuildInfoProvider.SetupGet(_ => _.Webclient).Returns(webclient).Verifiable();

            var mockWebHostEnvironment = new Mock<IWebHostEnvironment>(MockBehavior.Strict);
            mockWebHostEnvironment.SetupGet(_ => _.EnvironmentName).Returns("SomeEnvironmentName").Verifiable();

            var controller = new VersionController(mockBuildInfoProvider.Object, mockWebHostEnvironment.Object);

            var result = controller.Version();
            Assert.NotNull(result);

            var model = result.Value;
            Assert.NotNull(model);
            Assert.Equal("SomeEnvironmentName", model.Environment);
            Assert.Equal("SomeChangelist", model.Changelist);
            Assert.Equal("SomeBuildUrl", model.BuildUrl);
            Assert.Equal(webclient, model.Webclient);

            mockBuildInfoProvider.Verify();
            mockWebHostEnvironment.Verify();
        }
    }
}
