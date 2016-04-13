// <copyright file="ConfirmViewModelTests.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Tests.Models
{
    using ClickerHeroesTrackerWebsite.Models;
    using ClickerHeroesTrackerWebsite.Models.Calculator;
    using ClickerHeroesTrackerWebsite.Models.Game;
    using ClickerHeroesTrackerWebsite.Models.Settings;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mocks;
    using Moq;

    [TestClass]
    public class ConfirmViewModelTests
    {
        [Ignore]
        [TestMethod]
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

            Assert.IsTrue(viewModel.IsValid);
            Assert.IsFalse(viewModel.IsPublic);
            Assert.IsTrue(viewModel.IsOwn);
            Assert.IsTrue(viewModel.IsPermitted);

            Assert.IsNotNull(viewModel.AncientLevelSummaryViewModel);
            Assert.IsNotNull(viewModel.AncientLevelSummaryViewModel.AncientLevels);
            Assert.AreEqual(29, viewModel.AncientLevelSummaryViewModel.AncientLevels.Count);

            foreach (var pair in viewModel.AncientLevelSummaryViewModel.AncientLevels)
            {
                Assert.IsNotNull(pair);
                Assert.IsNotNull(pair.Key);
                Assert.IsTrue(pair.Value.EffectiveLevel >= 0);
            }

            /*
            Assert.IsNotNull(viewModel.HeroLevelSummaryViewModel);
            Assert.IsNotNull(viewModel.HeroLevelSummaryViewModel.HeroGilds);
            Assert.AreEqual(2, viewModel.HeroLevelSummaryViewModel.HeroGilds.Count);

            foreach (var pair in viewModel.HeroLevelSummaryViewModel.HeroGilds)
            {
                Assert.IsNotNull(pair);
                Assert.IsFalse(string.IsNullOrWhiteSpace(pair.Key));
                Assert.IsFalse(string.IsNullOrWhiteSpace(pair.Value));
            }
            */

            Assert.IsNotNull(viewModel.SuggestedAncientLevelsViewModel);
            Assert.IsNotNull(viewModel.SuggestedAncientLevelsViewModel.SuggestedAncientLevels);
            Assert.AreEqual(8, viewModel.SuggestedAncientLevelsViewModel.SuggestedAncientLevels.Length);

            foreach (var suggestedAncientLevel in viewModel.SuggestedAncientLevelsViewModel.SuggestedAncientLevels)
            {
                Assert.IsNotNull(suggestedAncientLevel);
                Assert.IsFalse(string.IsNullOrWhiteSpace(suggestedAncientLevel.AncientName));
                Assert.AreNotEqual(0, suggestedAncientLevel.LevelInfo.AncientLevel);
                Assert.AreNotEqual(0, suggestedAncientLevel.LevelInfo.ItemLevel);
                Assert.AreNotEqual(0, suggestedAncientLevel.LevelInfo.EffectiveLevel);
                Assert.AreNotEqual(0, suggestedAncientLevel.SuggestedLevel);
            }

            Assert.IsNotNull(viewModel.ComputedStatsViewModel);

            mockUserSettings.Verify();
            mockUserSettingsProvider.Verify();
        }
    }
}
