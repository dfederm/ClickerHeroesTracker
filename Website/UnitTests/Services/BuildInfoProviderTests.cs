// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System.Collections.Generic;
using System.IO;
using ClickerHeroesTrackerWebsite.Configuration;
using Microsoft.AspNetCore.Hosting;
using Moq;
using Newtonsoft.Json;
using UnitTests.Mocks;
using Xunit;

namespace UnitTests.Services
{
    public static class BuildInfoProviderTests
    {
        private static readonly Dictionary<string, string> BuildInfo = new()
        {
            { "changelist", "SomeChangelist" },
            { "buildUrl", "SomeBuildUrl" },
        };

        private static readonly Dictionary<string, string> Manifest = new()
        {
            { "runtime.js", "/39caf45e53fcc3060725.js" },
            { "runtime.js.map", "/39caf45e53fcc3060725.js.map" },
            { "vendors~app.js", "/7b01886e415366cd2f5c.js" },
            { "vendors~app.js.map", "/7b01886e415366cd2f5c.js.map" },
            { "data~app.js", "/20c89b387dbdb102dba5.js" },
            { "data~app.js.map", "/20c89b387dbdb102dba5.js.map" },
            { "app.js", "/d8df654c29474da7b106.js" },
            { "app.js.map", "/d8df654c29474da7b106.js.map" },
            { "index.html", "/index.html" },
        };

        [Fact]
        public static void BuildInfoProvider_Success()
        {
            using (TemporaryDirectory directory = new())
            {
                Mock<IWebHostEnvironment> mockWebHostEnvironment = new(MockBehavior.Strict);
                mockWebHostEnvironment.Setup(_ => _.WebRootPath).Returns(directory.Location).Verifiable();

                Directory.CreateDirectory(Path.Combine(directory.Location, "data"));
                File.WriteAllText(Path.Combine(directory.Location, "data", "BuildInfo.json"), JsonConvert.SerializeObject(BuildInfo));
                File.WriteAllText(Path.Combine(directory.Location, "manifest.json"), JsonConvert.SerializeObject(Manifest));

                BuildInfoProvider provider = new(mockWebHostEnvironment.Object);

                Assert.Equal("SomeBuildUrl", provider.BuildUrl);
                Assert.Equal("SomeChangelist", provider.Changelist);

                Assert.NotNull(provider.Webclient);
                Assert.Equal(4, provider.Webclient.Count);
                Assert.Equal("39caf45e53fcc3060725", provider.Webclient["runtime"]);
                Assert.Equal("7b01886e415366cd2f5c", provider.Webclient["vendors~app"]);
                Assert.Equal("20c89b387dbdb102dba5", provider.Webclient["data~app"]);
                Assert.Equal("d8df654c29474da7b106", provider.Webclient["app"]);

                mockWebHostEnvironment.Verify();
            }
        }

        [Fact]
        public static void BuildInfoProvider_EmptyBuildInfo()
        {
            using (TemporaryDirectory directory = new())
            {
                Mock<IWebHostEnvironment> mockWebHostEnvironment = new(MockBehavior.Strict);
                mockWebHostEnvironment.Setup(_ => _.WebRootPath).Returns(directory.Location).Verifiable();

                Directory.CreateDirectory(Path.Combine(directory.Location, "data"));
                File.WriteAllText(Path.Combine(directory.Location, "data", "BuildInfo.json"), "{}");
                File.WriteAllText(Path.Combine(directory.Location, "manifest.json"), JsonConvert.SerializeObject(Manifest));

                BuildInfoProvider provider = new(mockWebHostEnvironment.Object);

                Assert.Null(provider.BuildUrl);
                Assert.Null(provider.Changelist);

                Assert.NotNull(provider.Webclient);
                Assert.Equal(4, provider.Webclient.Count);
                Assert.Equal("39caf45e53fcc3060725", provider.Webclient["runtime"]);
                Assert.Equal("7b01886e415366cd2f5c", provider.Webclient["vendors~app"]);
                Assert.Equal("20c89b387dbdb102dba5", provider.Webclient["data~app"]);
                Assert.Equal("d8df654c29474da7b106", provider.Webclient["app"]);

                mockWebHostEnvironment.Verify();
            }
        }

        [Fact]
        public static void BuildInfoProvider_EmptyManifest()
        {
            using (TemporaryDirectory directory = new())
            {
                Mock<IWebHostEnvironment> mockWebHostEnvironment = new(MockBehavior.Strict);
                mockWebHostEnvironment.Setup(_ => _.WebRootPath).Returns(directory.Location).Verifiable();

                Directory.CreateDirectory(Path.Combine(directory.Location, "data"));
                File.WriteAllText(Path.Combine(directory.Location, "data", "BuildInfo.json"), JsonConvert.SerializeObject(BuildInfo));
                File.WriteAllText(Path.Combine(directory.Location, "manifest.json"), "{}");

                BuildInfoProvider provider = new(mockWebHostEnvironment.Object);

                Assert.Equal("SomeBuildUrl", provider.BuildUrl);
                Assert.Equal("SomeChangelist", provider.Changelist);

                Assert.NotNull(provider.Webclient);
                Assert.Equal(0, provider.Webclient.Count);

                mockWebHostEnvironment.Verify();
            }
        }

        [Fact]
        public static void BuildInfoProvider_MissingBuildInfo()
        {
            using (TemporaryDirectory directory = new())
            {
                Mock<IWebHostEnvironment> mockWebHostEnvironment = new(MockBehavior.Strict);
                mockWebHostEnvironment.Setup(_ => _.WebRootPath).Returns(directory.Location).Verifiable();

                File.WriteAllText(Path.Combine(directory.Location, "manifest.json"), JsonConvert.SerializeObject(Manifest));

                Assert.Throws<InvalidDataException>(() => new BuildInfoProvider(mockWebHostEnvironment.Object));

                mockWebHostEnvironment.Verify();
            }
        }

        [Fact]
        public static void BuildInfoProvider_MissingManifest()
        {
            using (TemporaryDirectory directory = new())
            {
                Mock<IWebHostEnvironment> mockWebHostEnvironment = new(MockBehavior.Strict);
                mockWebHostEnvironment.Setup(_ => _.WebRootPath).Returns(directory.Location).Verifiable();

                Directory.CreateDirectory(Path.Combine(directory.Location, "data"));
                File.WriteAllText(Path.Combine(directory.Location, "data", "BuildInfo.json"), JsonConvert.SerializeObject(BuildInfo));

                Assert.Throws<InvalidDataException>(() => new BuildInfoProvider(mockWebHostEnvironment.Object));

                mockWebHostEnvironment.Verify();
            }
        }
    }
}
