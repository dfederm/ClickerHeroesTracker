// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System.Collections.Generic;
using ClickerHeroesTrackerWebsite.Configuration;
using ClickerHeroesTrackerWebsite.Controllers;
using Microsoft.AspNetCore.Hosting;
using Moq;
using Xunit;

namespace ClickerHeroesTrackerWebsite.Tests.Controllers
{
    public static class VersionControllerTests
    {
        [Fact]
        public static void Version()
        {
            Mock<IBuildInfoProvider> mockBuildInfoProvider = new(MockBehavior.Strict);
            mockBuildInfoProvider.SetupGet(_ => _.Changelist).Returns("SomeChangelist").Verifiable();
            mockBuildInfoProvider.SetupGet(_ => _.BuildUrl).Returns("SomeBuildUrl").Verifiable();

            Dictionary<string, string> webclient = new()
            {
                { "bundle0", "version0" },
                { "bundle1", "version1" },
                { "bundle2", "version2" },
            };
            mockBuildInfoProvider.SetupGet(_ => _.Webclient).Returns(webclient).Verifiable();

            Mock<IWebHostEnvironment> mockWebHostEnvironment = new(MockBehavior.Strict);
            mockWebHostEnvironment.SetupGet(_ => _.EnvironmentName).Returns("SomeEnvironmentName").Verifiable();

            VersionController controller = new(mockBuildInfoProvider.Object, mockWebHostEnvironment.Object);

            Microsoft.AspNetCore.Mvc.ActionResult<Website.Models.Api.Version.VersionResponse> result = controller.Version();
            Assert.NotNull(result);

            Website.Models.Api.Version.VersionResponse model = result.Value;
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
