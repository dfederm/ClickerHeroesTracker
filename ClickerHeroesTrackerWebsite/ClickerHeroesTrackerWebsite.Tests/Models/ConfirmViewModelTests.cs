// <copyright file="ConfirmViewModelTests.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Tests.Models
{
    using ClickerHeroesTrackerWebsite.Models;
    using ClickerHeroesTrackerWebsite.Models.Calculator;
    using ClickerHeroesTrackerWebsite.Models.Settings;
    using Mocks;
    using Moq;
    using Xunit;

    public class ConfirmViewModelTests
    {
        [Fact(Skip = "Ignoring until we can properly mock the database for unit tests")]
        public void ConfirmViewModel_BasicFunctionalityTest()
        {
            var mockUserSettings = new Mock<IUserSettings>(MockBehavior.Strict);
            mockUserSettings.SetupGet(_ => _.AreUploadsPublic).Returns(false).Verifiable();
            mockUserSettings.SetupGet(_ => _.UseReducedSolomonFormula).Returns(false).Verifiable();
            mockUserSettings.SetupGet(_ => _.PlayStyle).Returns(PlayStyle.Idle).Verifiable();
            mockUserSettings.SetupGet(_ => _.UseExperimentalStats).Returns(false).Verifiable();

            var mockUserSettingsProvider = new Mock<IUserSettingsProvider>(MockBehavior.Strict);
            mockUserSettingsProvider.Setup(_ => _.Get(null)).Returns(mockUserSettings.Object).Verifiable();

            var viewModel = new CalculatorViewModel(
                null,
                mockUserSettingsProvider.Object,
                1234,
                null,
                null);

            Assert.True(viewModel.IsValid);
            Assert.False(viewModel.IsPublic);
            Assert.True(viewModel.IsOwn);
            Assert.True(viewModel.IsPermitted);

            mockUserSettings.Verify();
            mockUserSettingsProvider.Verify();
        }
    }
}
