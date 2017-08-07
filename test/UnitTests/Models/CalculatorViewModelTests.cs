// <copyright file="CalculatorViewModelTests.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Tests.Models
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Security.Claims;
    using ClickerHeroesTrackerWebsite.Models.Calculator;
    using ClickerHeroesTrackerWebsite.Models.Settings;
    using ClickerHeroesTrackerWebsite.Services.Database;
    using ClickerHeroesTrackerWebsite.Tests.Mocks;
    using Moq;
    using Xunit;

    public class CalculatorViewModelTests
    {
        [Fact]
        public void ConfirmViewModel_BasicFunctionalityTest()
        {
            const string UserId = "someUserId";

            var mockDataReader = new Mock<IDataReader>(MockBehavior.Strict);
            mockDataReader.Setup(_ => _.Read()).Returns(true).Verifiable();
            mockDataReader.Setup(_ => _.Dispose()).Verifiable();
            mockDataReader.SetupGet(_ => _["UserId"]).Returns(UserId).Verifiable();

            var mockDatabaseCommand = new Mock<IDatabaseCommand>(MockBehavior.Strict);
            mockDatabaseCommand.SetupSet(_ => _.CommandText = It.IsAny<string>()).Verifiable();
            mockDatabaseCommand.SetupSet(_ => _.Parameters = It.IsAny<IDictionary<string, object>>()).Verifiable();
            mockDatabaseCommand.Setup(_ => _.ExecuteReader()).Returns(mockDataReader.Object).Verifiable();
            mockDatabaseCommand.Setup(_ => _.Dispose()).Verifiable();

            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            mockDatabaseCommandFactory.Setup(_ => _.Create()).Returns(mockDatabaseCommand.Object).Verifiable();

            var mockUserSettings = new Mock<IUserSettings>(MockBehavior.Strict);
            mockUserSettings.SetupGet(_ => _.AreUploadsPublic).Returns(false).Verifiable();

            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
            mockUserSettingsProvider.Setup(_ => _.Get(UserId)).Returns(mockUserSettings.Object).Verifiable();

            var userManager = new MockUserManager();

            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, UserId) });
            var user = new ClaimsPrincipal(identity);

            var viewModel = new CalculatorViewModel(
                mockDatabaseCommandFactory.Object,
                mockUserSettingsProvider.Object,
                1234,
                user,
                userManager);

            Assert.True(viewModel.IsValid);
            Assert.False(viewModel.IsPublic);
            Assert.True(viewModel.IsOwn);
            Assert.True(viewModel.IsPermitted);

            mockDataReader.Verify();
            mockDatabaseCommand.Verify();
            mockDatabaseCommandFactory.Verify();
            mockUserSettings.Verify();
            mockUserSettingsProvider.Verify();
        }
    }
}
