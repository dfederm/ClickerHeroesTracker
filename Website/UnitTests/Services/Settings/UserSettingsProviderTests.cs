// <copyright file="UserSettingsProviderTests.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace UnitTests.Services.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ClickerHeroesTrackerWebsite.Models;
    using ClickerHeroesTrackerWebsite.Models.Settings;
    using ClickerHeroesTrackerWebsite.Services.Database;
    using ClickerHeroesTrackerWebsite.Tests.Mocks;
    using Moq;
    using Website.Models.Api.Users;
    using Xunit;

    public sealed class UserSettingsProviderTests
    {
        private const string UserId = "SomeUserId";

        [Fact]
        public void Get_Success()
        {
            var dataSets = CreateDataSetsForAllSettings();

            var mockDataReader = MockDatabaseHelper.CreateMockDataReader(dataSets);
            var mockDatabaseCommand = MockDatabaseHelper.CreateMockDatabaseCommand(new Dictionary<string, object> { { "@UserId", UserId } }, mockDataReader.Object);
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            mockDatabaseCommandFactory.Setup(_ => _.Create()).Returns(mockDatabaseCommand.Object).Verifiable();

            var provider = new UserSettingsProvider(mockDatabaseCommandFactory.Object);
            var settings = provider.Get(UserId);

            VerifyPopulatedSettings(settings);

            mockDataReader.VerifyAll();
            mockDatabaseCommand.VerifyAll();
            mockDatabaseCommandFactory.VerifyAll();
        }

        [Fact]
        public void Get_InvalidSettings()
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
            var settings = provider.Get(UserId);

            VerifyDefaultSettings(settings);

            mockDataReader.VerifyAll();
            mockDatabaseCommand.VerifyAll();
            mockDatabaseCommandFactory.VerifyAll();
        }

        [Fact]
        public void Get_MissingSettings()
        {
            var mockDataReader = MockDatabaseHelper.CreateMockDataReader();
            var mockDatabaseCommand = MockDatabaseHelper.CreateMockDatabaseCommand(new Dictionary<string, object> { { "@UserId", UserId } }, mockDataReader.Object);
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            mockDatabaseCommandFactory.Setup(_ => _.Create()).Returns(mockDatabaseCommand.Object).Verifiable();

            var provider = new UserSettingsProvider(mockDatabaseCommandFactory.Object);
            var settings = provider.Get(UserId);

            VerifyDefaultSettings(settings);

            mockDataReader.VerifyAll();
            mockDatabaseCommand.VerifyAll();
            mockDatabaseCommandFactory.VerifyAll();
        }

        [Fact]
        public void Get_UnknownSettings()
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
            var settings = provider.Get(UserId);

            VerifyPopulatedSettings(settings);

            mockDataReader.VerifyAll();
            mockDatabaseCommand.VerifyAll();
            mockDatabaseCommandFactory.VerifyAll();
        }

        [Fact]
        public void Get_DuplicateSettings()
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
            var settings = provider.Get(UserId);

            VerifyPopulatedSettings(settings);

            mockDataReader.VerifyAll();
            mockDatabaseCommand.VerifyAll();
            mockDatabaseCommandFactory.VerifyAll();
        }

        [Fact]
        public void Get_NoUser()
        {
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);

            var provider = new UserSettingsProvider(mockDatabaseCommandFactory.Object);
            var settings = provider.Get(null);

            VerifyDefaultSettings(settings);

            mockDatabaseCommandFactory.VerifyAll();
        }

        [Fact]
        public void Get_Cache()
        {
            var dataSets = CreateDataSetsForAllSettings();
            var mockDataReader = MockDatabaseHelper.CreateMockDataReader(dataSets);
            var mockDatabaseCommand = MockDatabaseHelper.CreateMockDatabaseCommand(new Dictionary<string, object> { { "@UserId", UserId } }, mockDataReader.Object);
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            mockDatabaseCommandFactory.Setup(_ => _.Create()).Returns(mockDatabaseCommand.Object).Verifiable();

            var provider = new UserSettingsProvider(mockDatabaseCommandFactory.Object);
            var settings = provider.Get(UserId);

            Assert.NotNull(settings);

            mockDataReader.VerifyAll();
            mockDatabaseCommand.VerifyAll();
            mockDatabaseCommandFactory.VerifyAll();

            // Reset all and go again
            mockDataReader.Reset();
            mockDatabaseCommand.Reset();
            mockDatabaseCommandFactory.Reset();

            var cachedSettings = provider.Get(UserId);

            Assert.Equal(settings, cachedSettings);

            mockDataReader.VerifyAll();
            mockDatabaseCommand.VerifyAll();
            mockDatabaseCommandFactory.VerifyAll();
        }

        [Fact]
        public void Patch_Success()
        {
            var settings = new UserSettings
            {
                AreUploadsPublic = true,
                PlayStyle = PlayStyle.Hybrid,
                UseScientificNotation = true,
                ScientificNotationThreshold = 123,
                UseEffectiveLevelForSuggestions = true,
                UseLogarithmicGraphScale = true,
                LogarithmicGraphScaleThreshold = 456,
                HybridRatio = 789,
                Theme = SiteThemeType.Dark,
            };

            var parameters = new Dictionary<string, object>
            {
                { "@UserId", UserId },
                { "@Value" + UserSettingsConstants.AreUploadsPublic, settings.AreUploadsPublic.ToString() },
                { "@Value" + UserSettingsConstants.PlayStyle, settings.PlayStyle.ToString() },
                { "@Value" + UserSettingsConstants.UseScientificNotation, settings.UseScientificNotation.ToString() },
                { "@Value" + UserSettingsConstants.ScientificNotationThreshold, settings.ScientificNotationThreshold.ToString() },
                { "@Value" + UserSettingsConstants.UseEffectiveLevelForSuggestions, settings.UseEffectiveLevelForSuggestions.ToString() },
                { "@Value" + UserSettingsConstants.UseLogarithmicGraphScale, settings.UseLogarithmicGraphScale.ToString() },
                { "@Value" + UserSettingsConstants.LogarithmicGraphScaleThreshold, settings.LogarithmicGraphScaleThreshold.ToString() },
                { "@Value" + UserSettingsConstants.HybridRatio, settings.HybridRatio.ToString() },
                { "@Value" + UserSettingsConstants.Theme, settings.Theme.ToString() },
            };

            var mockDatabaseCommand = MockDatabaseHelper.CreateMockDatabaseCommand(parameters);
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            mockDatabaseCommandFactory.Setup(_ => _.Create()).Returns(mockDatabaseCommand.Object).Verifiable();

            var provider = new UserSettingsProvider(mockDatabaseCommandFactory.Object);
            provider.Patch(UserId, settings);

            mockDatabaseCommand.VerifyAll();
            mockDatabaseCommandFactory.VerifyAll();
        }

        [Fact]
        public void Patch_PartialSettings()
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
            provider.Patch(UserId, settings);

            mockDatabaseCommand.VerifyAll();
            mockDatabaseCommandFactory.VerifyAll();
        }

        [Fact]
        public void Patch_EmptySettings()
        {
            var settings = new UserSettings();

            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);

            var provider = new UserSettingsProvider(mockDatabaseCommandFactory.Object);
            provider.Patch(UserId, settings);

            mockDatabaseCommandFactory.VerifyAll();
        }

        [Fact]
        public void Patch_NoUser()
        {
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var provider = new UserSettingsProvider(mockDatabaseCommandFactory.Object);
            var settings = new UserSettings();

            Assert.Throws<ArgumentNullException>("userId", () => provider.Patch(null, settings));
        }

        [Fact]
        public void Patch_NoSettings()
        {
            var mockDatabaseCommandFactory = new Mock<IDatabaseCommandFactory>(MockBehavior.Strict);
            var provider = new UserSettingsProvider(mockDatabaseCommandFactory.Object);

            Assert.Throws<ArgumentNullException>("userSettings", () => provider.Patch(UserId, null));
        }

        private static List<IDictionary<string, object>> CreateDataSetsForAllSettings()
        {
            return new IDictionary<string, object>[]
            {
                new Dictionary<string, object> { { "SettingId", UserSettingsConstants.AreUploadsPublic }, { "SettingValue", "true" } },
                new Dictionary<string, object> { { "SettingId", UserSettingsConstants.PlayStyle }, { "SettingValue", "Hybrid" } },
                new Dictionary<string, object> { { "SettingId", UserSettingsConstants.UseScientificNotation }, { "SettingValue", "true" } },
                new Dictionary<string, object> { { "SettingId", UserSettingsConstants.ScientificNotationThreshold }, { "SettingValue", "123" } },
                new Dictionary<string, object> { { "SettingId", UserSettingsConstants.UseEffectiveLevelForSuggestions }, { "SettingValue", "true" } },
                new Dictionary<string, object> { { "SettingId", UserSettingsConstants.UseLogarithmicGraphScale }, { "SettingValue", "true" } },
                new Dictionary<string, object> { { "SettingId", UserSettingsConstants.LogarithmicGraphScaleThreshold }, { "SettingValue", "456" } },
                new Dictionary<string, object> { { "SettingId", UserSettingsConstants.HybridRatio }, { "SettingValue", "789" } },
                new Dictionary<string, object> { { "SettingId", UserSettingsConstants.Theme }, { "SettingValue", "Dark" } },
            }.ToList();
        }

        private static void VerifyPopulatedSettings(UserSettings settings)
        {
            Assert.NotNull(settings);
            Assert.True(settings.AreUploadsPublic);
            Assert.Equal(PlayStyle.Hybrid, settings.PlayStyle);
            Assert.True(settings.UseScientificNotation);
            Assert.Equal(123, settings.ScientificNotationThreshold);
            Assert.True(settings.UseEffectiveLevelForSuggestions);
            Assert.True(settings.UseLogarithmicGraphScale);
            Assert.Equal(456, settings.LogarithmicGraphScaleThreshold);
            Assert.Equal(789, settings.HybridRatio);
            Assert.Equal(SiteThemeType.Dark, settings.Theme);
        }

        private static void VerifyDefaultSettings(UserSettings settings)
        {
            Assert.NotNull(settings);
            Assert.Null(settings.AreUploadsPublic);
            Assert.Null(settings.PlayStyle);
            Assert.Null(settings.UseScientificNotation);
            Assert.Null(settings.ScientificNotationThreshold);
            Assert.Null(settings.UseEffectiveLevelForSuggestions);
            Assert.Null(settings.UseLogarithmicGraphScale);
            Assert.Null(settings.LogarithmicGraphScaleThreshold);
            Assert.Null(settings.HybridRatio);
            Assert.Null(settings.Theme);
        }
    }
}
