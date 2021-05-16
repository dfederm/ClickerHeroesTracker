// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System.IO;
using ClickerHeroesTrackerWebsite.Configuration;
using UnitTests.Mocks;
using Xunit;

namespace UnitTests.Services
{
    public static class FileBuildInfoProviderTests
    {
        [Fact]
        public static void BuildInfoProvider_Success()
        {
            using TemporaryDirectory directory = new();
            const string BuildInfo = @"
{
    ""changelist"": ""SomeChangelist"",
    ""buildUrl"": ""SomeBuildUrl"",
    ""webclient"": {
        ""file1"": ""hash1"",
        ""file2"": ""hash2"",
        ""file3"": ""hash3""
    }
}
            ";

            string buildInfoFile = Path.Combine(directory.Location, "BuildInfo.json");
            File.WriteAllText(buildInfoFile, BuildInfo);

            FileBuildInfoProvider provider = new(buildInfoFile);

            Assert.Equal("SomeBuildUrl", provider.BuildUrl);
            Assert.Equal("SomeChangelist", provider.Changelist);

            Assert.NotNull(provider.Webclient);
            Assert.Equal(3, provider.Webclient.Count);
            Assert.Equal("hash1", provider.Webclient["file1"]);
            Assert.Equal("hash2", provider.Webclient["file2"]);
            Assert.Equal("hash3", provider.Webclient["file3"]);
        }

        [Fact]
        public static void BuildInfoProvider_EmptyBuildInfo()
        {
            using TemporaryDirectory directory = new();
            string buildInfoFile = Path.Combine(directory.Location, "BuildInfo.json");
            File.WriteAllText(buildInfoFile, "{}");

            FileBuildInfoProvider provider = new(buildInfoFile);

            Assert.Null(provider.BuildUrl);
            Assert.Null(provider.Changelist);
            Assert.Null(provider.Webclient);
        }

        [Fact]
        public static void BuildInfoProvider_MissingBuildInfo()
        {
            using TemporaryDirectory directory = new();
            string buildInfoFile = Path.Combine(directory.Location, "BuildInfo.json");

            Assert.Throws<InvalidDataException>(() => new FileBuildInfoProvider(buildInfoFile));
        }
    }
}
