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
                MockGameData.RealData,
                null,
                1234,
                null);

            Assert.True(viewModel.IsValid);
            Assert.False(viewModel.IsPublic);
            Assert.True(viewModel.IsOwn);
            Assert.True(viewModel.IsPermitted);

            Assert.NotNull(viewModel.AncientLevelSummaryViewModel);
            Assert.NotNull(viewModel.AncientLevelSummaryViewModel.AncientLevels);
            Assert.Equal(29, viewModel.AncientLevelSummaryViewModel.AncientLevels.Count);

            foreach (var pair in viewModel.AncientLevelSummaryViewModel.AncientLevels)
            {
                Assert.NotNull(pair);
                Assert.NotNull(pair.Key);
                Assert.True(pair.Value.EffectiveLevel >= 0);
            }

            /*
            Assert.NotNull(viewModel.HeroLevelSummaryViewModel);
            Assert.NotNull(viewModel.HeroLevelSummaryViewModel.HeroGilds);
            Assert.Equal(2, viewModel.HeroLevelSummaryViewModel.HeroGilds.Count);

            foreach (var pair in viewModel.HeroLevelSummaryViewModel.HeroGilds)
            {
                Assert.NotNull(pair);
                Assert.False(string.IsNullOrWhiteSpace(pair.Key));
                Assert.False(string.IsNullOrWhiteSpace(pair.Value));
            }
            */

            Assert.NotNull(viewModel.SuggestedAncientLevelsViewModel);
            Assert.NotNull(viewModel.SuggestedAncientLevelsViewModel.SuggestedAncientLevels);
            Assert.Equal(8, viewModel.SuggestedAncientLevelsViewModel.SuggestedAncientLevels.Length);

            foreach (var suggestedAncientLevel in viewModel.SuggestedAncientLevelsViewModel.SuggestedAncientLevels)
            {
                Assert.NotNull(suggestedAncientLevel);
                Assert.False(string.IsNullOrWhiteSpace(suggestedAncientLevel.AncientName));
                Assert.NotEqual(0, suggestedAncientLevel.LevelInfo.AncientLevel);
                Assert.NotEqual(0, suggestedAncientLevel.LevelInfo.ItemLevel);
                Assert.NotEqual(0, suggestedAncientLevel.LevelInfo.EffectiveLevel);
                Assert.NotEqual(0, suggestedAncientLevel.SuggestedLevel);
            }

            Assert.NotNull(viewModel.ComputedStatsViewModel);

            mockUserSettings.Verify();
            mockUserSettingsProvider.Verify();
        }
    }
}
