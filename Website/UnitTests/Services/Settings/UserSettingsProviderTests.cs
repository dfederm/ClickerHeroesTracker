// <copyright file="UserSettingsProviderTests.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace UnitTests.Services.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using ClickerHeroesTrackerWebsite.Models;
    using ClickerHeroesTrackerWebsite.Models.Settings;
    using ClickerHeroesTrackerWebsite.Services.Database;
    using ClickerHeroesTrackerWebsite.Tests.Mocks;
    using Moq;
    using Website.Models.Api.Users;
    using Xunit;

    public static class UserSettingsProviderTests
    {
        private const string UserId = "SomeUserId";

        [Fact]
        public static async Task Get_Success()
        {
            var dataSets = CreateDataSetsForAllSettings();

            var mockDataReader = MockDatabaseHelper.CreateMockDataReader(dataSets);
            var mockDatabaseCommand = MockDatabaseHelper.CreateMockDatabaseCommand(new Dictionary<string, object> { { "@UserId", UserId } }, mockDataReader.Object);
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            mockDatabaseCommandFactory.Setup(_ => _.Create()).Returns(mockDatabaseCommand.Object).Verifiable();

            var provider = new UserSettingsProvider(mockDatabaseCommandFactory.Object);
            var settings = await provider.GetAsync(UserId);

            VerifyPopulatedSettings(settings);

            mockDataReader.VerifyAll();
            mockDatabaseCommand.VerifyAll();
            mockDatabaseCommandFactory.VerifyAll();
        }

        [Fact]
        public static async Task Get_InvalidSettings()
        {
            var dataSets = new List<IDictionary<string, object>>();
            foreach (var dataSet in CreateDataSetsForAllSettings())
            {
                dataSet["SettingValue"] = "SomethingInvalid";
                dataSets.Add(dataSet);
            }

            var mockDataReader = MockDatabaseHelper.CreateMockDataReader(dataSets);
            var mockDatabaseCommand = MockDatabaseHelper.CreateMockDatabaseCommand(new Dictionary<string, object> { { "@UserId", UserId } }, mockDataReader.Object);
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            mockDatabaseCommandFactory.Setup(_ => _.Create()).Returns(mockDatabaseCommand.Object).Verifiable();

            var provider = new UserSettingsProvider(mockDatabaseCommandFactory.Object);
            var settings = await provider.GetAsync(UserId);

            VerifyDefaultSettings(settings);

            mockDataReader.VerifyAll();
            mockDatabaseCommand.VerifyAll();
            mockDatabaseCommandFactory.VerifyAll();
        }

        [Fact]
        public static async Task Get_MissingSettings()
        {
            var mockDataReader = MockDatabaseHelper.CreateMockDataReader();
            var mockDatabaseCommand = MockDatabaseHelper.CreateMockDatabaseCommand(new Dictionary<string, object> { { "@UserId", UserId } }, mockDataReader.Object);
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            mockDatabaseCommandFactory.Setup(_ => _.Create()).Returns(mockDatabaseCommand.Object).Verifiable();

            var provider = new UserSettingsProvider(mockDatabaseCommandFactory.Object);
            var settings = await provider.GetAsync(UserId);

            VerifyDefaultSettings(settings);

            mockDataReader.VerifyAll();
            mockDatabaseCommand.VerifyAll();
            mockDatabaseCommandFactory.VerifyAll();
        }

        [Fact]
        public static async Task Get_UnknownSettings()
        {
            var dataSets = new List<IDictionary<string, object>>();
            var unknownSetting = new Dictionary<string, object> { { "SettingId", byte.MaxValue }, { "SettingValue", "true" } };
            dataSets.Add(unknownSetting);
            foreach (var dataSet in CreateDataSetsForAllSettings())
            {
                dataSets.Add(dataSet);
                dataSets.Add(unknownSetting);
            }

            var mockDataReader = MockDatabaseHelper.CreateMockDataReader(dataSets);
            var mockDatabaseCommand = MockDatabaseHelper.CreateMockDatabaseCommand(new Dictionary<string, object> { { "@UserId", UserId } }, mockDataReader.Object);
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            mockDatabaseCommandFactory.Setup(_ => _.Create()).Returns(mockDatabaseCommand.Object).Verifiable();

            var provider = new UserSettingsProvider(mockDatabaseCommandFactory.Object);
            var settings = await provider.GetAsync(UserId);

            VerifyPopulatedSettings(settings);

            mockDataReader.VerifyAll();
            mockDatabaseCommand.VerifyAll();
            mockDatabaseCommandFactory.VerifyAll();
        }

        [Fact]
        public static async Task Get_DuplicateSettings()
        {
            var dataSets = new List<IDictionary<string, object>>();
            foreach (var dataSet in CreateDataSetsForAllSettings())
            {
                dataSets.Add(dataSet);
                dataSets.Add(dataSet);
            }

            var mockDataReader = MockDatabaseHelper.CreateMockDataReader(dataSets);
            var mockDatabaseCommand = MockDatabaseHelper.CreateMockDatabaseCommand(new Dictionary<string, object> { { "@UserId", UserId } }, mockDataReader.Object);
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            mockDatabaseCommandFactory.Setup(_ => _.Create()).Returns(mockDatabaseCommand.Object).Verifiable();

            var provider = new UserSettingsProvider(mockDatabaseCommandFactory.Object);
            var settings = await provider.GetAsync(UserId);

            VerifyPopulatedSettings(settings);

            mockDataReader.VerifyAll();
            mockDatabaseCommand.VerifyAll();
            mockDatabaseCommandFactory.VerifyAll();
        }

        [Fact]
        public static async Task Get_NoUser()
        {
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);

            var provider = new UserSettingsProvider(mockDatabaseCommandFactory.Object);
            var settings = await provider.GetAsync(null);

            VerifyDefaultSettings(settings);

            mockDatabaseCommandFactory.VerifyAll();
        }

        [Fact]
        public static async Task Patch_Success()
        {
            var settings = new UserSettings
            {
                PlayStyle = PlayStyle.Hybrid,
                UseScientificNotation = true,
                ScientificNotationThreshold = 1,
                UseLogarithmicGraphScale = true,
                LogarithmicGraphScaleThreshold = 2,
                HybridRatio = 3,
                Theme = SiteThemeType.Dark,
                ShouldLevelSkillAncients = true,
                SkillAncientBaseAncient = 4,
                SkillAncientLevelDiff = 5,
            };

            var parameters = new Dictionary<string, object>
            {
                { "@UserId", UserId },
                { "@Value" + UserSettingsConstants.PlayStyle, settings.PlayStyle.ToString() },
                { "@Value" + UserSettingsConstants.UseScientificNotation, settings.UseScientificNotation.ToString() },
                { "@Value" + UserSettingsConstants.ScientificNotationThreshold, settings.ScientificNotationThreshold.ToString() },
                { "@Value" + UserSettingsConstants.UseLogarithmicGraphScale, settings.UseLogarithmicGraphScale.ToString() },
                { "@Value" + UserSettingsConstants.LogarithmicGraphScaleThreshold, settings.LogarithmicGraphScaleThreshold.ToString() },
                { "@Value" + UserSettingsConstants.HybridRatio, settings.HybridRatio.ToString() },
                { "@Value" + UserSettingsConstants.Theme, settings.Theme.ToString() },
                { "@Value" + UserSettingsConstants.ShouldLevelSkillAncients, settings.ShouldLevelSkillAncients.ToString() },
                { "@Value" + UserSettingsConstants.SkillAncientBaseAncient, settings.SkillAncientBaseAncient.ToString() },
                { "@Value" + UserSettingsConstants.SkillAncientLevelDiff, settings.SkillAncientLevelDiff.ToString() },
            };

            var mockDatabaseCommand = MockDatabaseHelper.CreateMockDatabaseCommand(parameters);
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            mockDatabaseCommandFactory.Setup(_ => _.Create()).Returns(mockDatabaseCommand.Object).Verifiable();

            var provider = new UserSettingsProvider(mockDatabaseCommandFactory.Object);
            await provider.PatchAsync(UserId, settings);

            mockDatabaseCommand.VerifyAll();
            mockDatabaseCommandFactory.VerifyAll();
        }

        [Fact]
        public static async Task Patch_PartialSettings()
        {
            var settings = new UserSettings
            {
                PlayStyle = PlayStyle.Hybrid,
            };

            var parameters = new Dictionary<string, object>
            {
                { "@UserId", UserId },
                { "@Value" + UserSettingsConstants.PlayStyle, settings.PlayStyle.ToString() },
            };

            var mockDatabaseCommand = MockDatabaseHelper.CreateMockDatabaseCommand(parameters);
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            mockDatabaseCommandFactory.Setup(_ => _.Create()).Returns(mockDatabaseCommand.Object).Verifiable();

            var provider = new UserSettingsProvider(mockDatabaseCommandFactory.Object);
            await provider.PatchAsync(UserId, settings);

            mockDatabaseCommand.VerifyAll();
            mockDatabaseCommandFactory.VerifyAll();
        }

        [Fact]
        public static async Task Patch_EmptySettings()
        {
            var settings = new UserSettings();

            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);

            var provider = new UserSettingsProvider(mockDatabaseCommandFactory.Object);
            await provider.PatchAsync(UserId, settings);

            mockDatabaseCommandFactory.VerifyAll();
        }

        [Fact]
        public static async Task Patch_NoUser()
        {
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var provider = new UserSettingsProvider(mockDatabaseCommandFactory.Object);
            var settings = new UserSettings();

            await Assert.ThrowsAsync<ArgumentNullException>("userId", () => provider.PatchAsync(null, settings));
        }

        [Fact]
        public static async Task Patch_NoSettings()
        {
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var provider = new UserSettingsProvider(mockDatabaseCommandFactory.Object);

            await Assert.ThrowsAsync<ArgumentNullException>("userSettings", () => provider.PatchAsync(UserId, null));
        }

        private static List<IDictionary<string, object>> CreateDataSetsForAllSettings()
        {
            return new IDictionary<string, object>[]
            {
                new Dictionary<string, object> { { "SettingId", UserSettingsConstants.PlayStyle }, { "SettingValue", "Hybrid" } },
                new Dictionary<string, object> { { "SettingId", UserSettingsConstants.UseScientificNotation }, { "SettingValue", "true" } },
                new Dictionary<string, object> { { "SettingId", UserSettingsConstants.ScientificNotationThreshold }, { "SettingValue", "1" } },
                new Dictionary<string, object> { { "SettingId", UserSettingsConstants.UseLogarithmicGraphScale }, { "SettingValue", "true" } },
                new Dictionary<string, object> { { "SettingId", UserSettingsConstants.LogarithmicGraphScaleThreshold }, { "SettingValue", "2" } },
                new Dictionary<string, object> { { "SettingId", UserSettingsConstants.HybridRatio }, { "SettingValue", "3" } },
                new Dictionary<string, object> { { "SettingId", UserSettingsConstants.Theme }, { "SettingValue", "Dark" } },
                new Dictionary<string, object> { { "SettingId", UserSettingsConstants.ShouldLevelSkillAncients }, { "SettingValue", "true" } },
                new Dictionary<string, object> { { "SettingId", UserSettingsConstants.SkillAncientBaseAncient }, { "SettingValue", "4" } },
                new Dictionary<string, object> { { "SettingId", UserSettingsConstants.SkillAncientLevelDiff }, { "SettingValue", "5" } },
            }.ToList();
        }

        private static void VerifyPopulatedSettings(UserSettings settings)
        {
            Assert.NotNull(settings);
            Assert.Equal(PlayStyle.Hybrid, settings.PlayStyle);
            Assert.True(settings.UseScientificNotation);
            Assert.Equal(1, settings.ScientificNotationThreshold);
            Assert.True(settings.UseLogarithmicGraphScale);
            Assert.Equal(2, settings.LogarithmicGraphScaleThreshold);
            Assert.Equal(3, settings.HybridRatio);
            Assert.Equal(SiteThemeType.Dark, settings.Theme);
            Assert.True(settings.ShouldLevelSkillAncients);
            Assert.Equal(4, settings.SkillAncientBaseAncient);
            Assert.Equal(5, settings.SkillAncientLevelDiff);
        }

        private static void VerifyDefaultSettings(UserSettings settings)
        {
            Assert.NotNull(settings);
            Assert.Null(settings.PlayStyle);
            Assert.Null(settings.UseScientificNotation);
            Assert.Null(settings.ScientificNotationThreshold);
            Assert.Null(settings.UseLogarithmicGraphScale);
            Assert.Null(settings.LogarithmicGraphScaleThreshold);
            Assert.Null(settings.HybridRatio);
            Assert.Null(settings.Theme);
            Assert.Null(settings.ShouldLevelSkillAncients);
            Assert.Null(settings.SkillAncientBaseAncient);
            Assert.Null(settings.SkillAncientLevelDiff);
        }
    }
}
