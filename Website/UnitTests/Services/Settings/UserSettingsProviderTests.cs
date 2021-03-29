// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ClickerHeroesTrackerWebsite.Models;
using ClickerHeroesTrackerWebsite.Models.Settings;
using ClickerHeroesTrackerWebsite.Services.Database;
using ClickerHeroesTrackerWebsite.Tests.Mocks;
using Moq;
using Website.Models.Api.Users;
using Website.Services.Settings;
using Xunit;

namespace UnitTests.Services.Settings
{
    public static class UserSettingsProviderTests
    {
        private const string UserId = "SomeUserId";

        [Fact]
        public static async Task Get_Success()
        {
            List<IDictionary<string, object>> dataSets = CreateDataSetsForAllSettings();

            Mock<IDataReader> mockDataReader = MockDatabaseHelper.CreateMockDataReader(dataSets);
            Mock<IDatabaseCommand> mockDatabaseCommand = MockDatabaseHelper.CreateMockDatabaseCommand(new Dictionary<string, object> { { "@UserId", UserId } }, mockDataReader.Object);
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            mockDatabaseCommandFactory.Setup(_ => _.Create()).Returns(mockDatabaseCommand.Object).Verifiable();

            UserSettingsProvider provider = new(mockDatabaseCommandFactory.Object);
            UserSettings settings = await provider.GetAsync(UserId);

            VerifyPopulatedSettings(settings);

            mockDataReader.VerifyAll();
            mockDatabaseCommand.VerifyAll();
            mockDatabaseCommandFactory.VerifyAll();
        }

        [Fact]
        public static async Task Get_InvalidSettings()
        {
            List<IDictionary<string, object>> dataSets = new();
            foreach (IDictionary<string, object> dataSet in CreateDataSetsForAllSettings())
            {
                dataSet["SettingValue"] = "SomethingInvalid";
                dataSets.Add(dataSet);
            }

            Mock<IDataReader> mockDataReader = MockDatabaseHelper.CreateMockDataReader(dataSets);
            Mock<IDatabaseCommand> mockDatabaseCommand = MockDatabaseHelper.CreateMockDatabaseCommand(new Dictionary<string, object> { { "@UserId", UserId } }, mockDataReader.Object);
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            mockDatabaseCommandFactory.Setup(_ => _.Create()).Returns(mockDatabaseCommand.Object).Verifiable();

            UserSettingsProvider provider = new(mockDatabaseCommandFactory.Object);
            UserSettings settings = await provider.GetAsync(UserId);

            VerifyDefaultSettings(settings);

            mockDataReader.VerifyAll();
            mockDatabaseCommand.VerifyAll();
            mockDatabaseCommandFactory.VerifyAll();
        }

        [Fact]
        public static async Task Get_MissingSettings()
        {
            Mock<IDataReader> mockDataReader = MockDatabaseHelper.CreateMockDataReader();
            Mock<IDatabaseCommand> mockDatabaseCommand = MockDatabaseHelper.CreateMockDatabaseCommand(new Dictionary<string, object> { { "@UserId", UserId } }, mockDataReader.Object);
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            mockDatabaseCommandFactory.Setup(_ => _.Create()).Returns(mockDatabaseCommand.Object).Verifiable();

            UserSettingsProvider provider = new(mockDatabaseCommandFactory.Object);
            UserSettings settings = await provider.GetAsync(UserId);

            VerifyDefaultSettings(settings);

            mockDataReader.VerifyAll();
            mockDatabaseCommand.VerifyAll();
            mockDatabaseCommandFactory.VerifyAll();
        }

        [Fact]
        public static async Task Get_UnknownSettings()
        {
            List<IDictionary<string, object>> dataSets = new();
            Dictionary<string, object> unknownSetting = new() { { "SettingId", byte.MaxValue }, { "SettingValue", "true" } };
            dataSets.Add(unknownSetting);
            foreach (IDictionary<string, object> dataSet in CreateDataSetsForAllSettings())
            {
                dataSets.Add(dataSet);
                dataSets.Add(unknownSetting);
            }

            Mock<IDataReader> mockDataReader = MockDatabaseHelper.CreateMockDataReader(dataSets);
            Mock<IDatabaseCommand> mockDatabaseCommand = MockDatabaseHelper.CreateMockDatabaseCommand(new Dictionary<string, object> { { "@UserId", UserId } }, mockDataReader.Object);
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            mockDatabaseCommandFactory.Setup(_ => _.Create()).Returns(mockDatabaseCommand.Object).Verifiable();

            UserSettingsProvider provider = new(mockDatabaseCommandFactory.Object);
            UserSettings settings = await provider.GetAsync(UserId);

            VerifyPopulatedSettings(settings);

            mockDataReader.VerifyAll();
            mockDatabaseCommand.VerifyAll();
            mockDatabaseCommandFactory.VerifyAll();
        }

        [Fact]
        public static async Task Get_DuplicateSettings()
        {
            List<IDictionary<string, object>> dataSets = new();
            foreach (IDictionary<string, object> dataSet in CreateDataSetsForAllSettings())
            {
                dataSets.Add(dataSet);
                dataSets.Add(dataSet);
            }

            Mock<IDataReader> mockDataReader = MockDatabaseHelper.CreateMockDataReader(dataSets);
            Mock<IDatabaseCommand> mockDatabaseCommand = MockDatabaseHelper.CreateMockDatabaseCommand(new Dictionary<string, object> { { "@UserId", UserId } }, mockDataReader.Object);
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            mockDatabaseCommandFactory.Setup(_ => _.Create()).Returns(mockDatabaseCommand.Object).Verifiable();

            UserSettingsProvider provider = new(mockDatabaseCommandFactory.Object);
            UserSettings settings = await provider.GetAsync(UserId);

            VerifyPopulatedSettings(settings);

            mockDataReader.VerifyAll();
            mockDatabaseCommand.VerifyAll();
            mockDatabaseCommandFactory.VerifyAll();
        }

        [Fact]
        public static async Task Get_NoUser()
        {
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);

            UserSettingsProvider provider = new(mockDatabaseCommandFactory.Object);
            UserSettings settings = await provider.GetAsync(null);

            VerifyDefaultSettings(settings);

            mockDatabaseCommandFactory.VerifyAll();
        }

        [Fact]
        public static async Task Patch_Success()
        {
            UserSettings settings = new()
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
                GraphSpacingType = GraphSpacingType.Ascension,
            };

            Dictionary<string, object> parameters = new()
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
                { "@Value" + UserSettingsConstants.GraphSpacingType, settings.GraphSpacingType.ToString() },
            };

            Mock<IDatabaseCommand> mockDatabaseCommand = MockDatabaseHelper.CreateMockDatabaseCommand(parameters);
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            mockDatabaseCommandFactory.Setup(_ => _.Create()).Returns(mockDatabaseCommand.Object).Verifiable();

            UserSettingsProvider provider = new(mockDatabaseCommandFactory.Object);
            await provider.PatchAsync(UserId, settings);

            mockDatabaseCommand.VerifyAll();
            mockDatabaseCommandFactory.VerifyAll();
        }

        [Fact]
        public static async Task Patch_PartialSettings()
        {
            UserSettings settings = new()
            {
                PlayStyle = PlayStyle.Hybrid,
            };

            Dictionary<string, object> parameters = new()
            {
                { "@UserId", UserId },
                { "@Value" + UserSettingsConstants.PlayStyle, settings.PlayStyle.ToString() },
            };

            Mock<IDatabaseCommand> mockDatabaseCommand = MockDatabaseHelper.CreateMockDatabaseCommand(parameters);
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            mockDatabaseCommandFactory.Setup(_ => _.Create()).Returns(mockDatabaseCommand.Object).Verifiable();

            UserSettingsProvider provider = new(mockDatabaseCommandFactory.Object);
            await provider.PatchAsync(UserId, settings);

            mockDatabaseCommand.VerifyAll();
            mockDatabaseCommandFactory.VerifyAll();
        }

        [Fact]
        public static async Task Patch_EmptySettings()
        {
            UserSettings settings = new();

            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);

            UserSettingsProvider provider = new(mockDatabaseCommandFactory.Object);
            await provider.PatchAsync(UserId, settings);

            mockDatabaseCommandFactory.VerifyAll();
        }

        [Fact]
        public static async Task Patch_NoUser()
        {
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            UserSettingsProvider provider = new(mockDatabaseCommandFactory.Object);
            UserSettings settings = new();

            await Assert.ThrowsAsync<ArgumentNullException>("userId", () => provider.PatchAsync(null, settings));
        }

        [Fact]
        public static async Task Patch_NoSettings()
        {
            Mock<IDatabaseCommandFactory> mockDatabaseCommandFactory = new(MockBehavior.Strict);
            UserSettingsProvider provider = new(mockDatabaseCommandFactory.Object);

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
                new Dictionary<string, object> { { "SettingId", UserSettingsConstants.GraphSpacingType }, { "SettingValue", "Ascension" } },
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
            Assert.Equal(GraphSpacingType.Ascension, settings.GraphSpacingType);
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
            Assert.Null(settings.GraphSpacingType);
        }
    }
}
