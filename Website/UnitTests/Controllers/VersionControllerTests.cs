// <copyright file="VersionControllerTests.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Tests.Controllers
{
    using System.Collections.Generic;
    using ClickerHeroesTrackerWebsite.Configuration;
    using ClickerHeroesTrackerWebsite.Controllers;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using Xunit;

    public class VersionControllerTests
    {
        [Fact]
        public void Version()
        {
            var mockBuildInfoProvider = new Mock<IBuildInfoProvider>(MockBehavior.Strict);
            mockBuildInfoProvider.SetupGet(_ => _.Changelist).Returns("SomeChangelist").Verifiable();
            mockBuildInfoProvider.SetupGet(_ => _.BuildId).Returns("SomeBuildId").Verifiable();

            var webclient = new Dictionary<string, string>
            {
                { "bundle0", "version0" },
                { "bundle1", "version1" },
                { "bundle2", "version2" },
            };
            mockBuildInfoProvider.SetupGet(_ => _.Webclient).Returns(webclient).Verifiable();

            var mockHostingEnvironment = new Mock<IHostingEnvironment>(MockBehavior.Strict);
            mockHostingEnvironment.SetupGet(_ => _.EnvironmentName).Returns("SomeEnvironmentName").Verifiable();

            var controller = new VersionController(mockBuildInfoProvider.Object, mockHostingEnvironment.Object);

            var result = controller.Version();

            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);

            // It's an anonymous type, so use dynamic to get the props off it.
            dynamic model = ((OkObjectResult)result).Value;
            Assert.NotNull(model);
            Assert.Equal("SomeEnvironmentName", model.Environment);
            Assert.Equal("SomeChangelist", model.Changelist);
            Assert.Equal("SomeBuildId", model.BuildId);
            Assert.Equal(webclient, model.Webclient);

            mockBuildInfoProvider.Verify();
            mockHostingEnvironment.Verify();
        }
    }
}
