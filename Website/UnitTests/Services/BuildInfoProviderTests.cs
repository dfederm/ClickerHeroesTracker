// <copyright file="BuildInfoProviderTests.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace UnitTests.Services
{
    using System.Collections.Generic;
    using System.IO;
    using ClickerHeroesTrackerWebsite.Configuration;
    using Microsoft.AspNetCore.Hosting;
    using Moq;
    using Newtonsoft.Json;
    using UnitTests.Mocks;
    using Xunit;

    public class BuildInfoProviderTests
    {
        private static Dictionary<string, string> buildInfo = new Dictionary<string, string>
        {
            { "changelist", "SomeChangelist" },
            { "buildId", "SomeBuildId" },
        };

        private static Dictionary<string, string> manifestNoHashes = new Dictionary<string, string>
        {
            { "bundle0.js", "bundle0.js" },
            { "bundle1.js", "bundle1.js" },
            { "bundle2.js", "bundle2.js" },
        };

        private static Dictionary<string, string> manifestWithHashes = new Dictionary<string, string>
        {
            { "bundle0.js", "bundle0.a.js" },
            { "bundle1.js", "bundle1.b.js" },
            { "bundle2.js", "bundle2.c.js" },
        };

        [Fact]
        public void BuildInfoProvider_NoHashes()
        {
            using (var directory = new TemporaryDirectory())
            {
                var mockHostingEnvironment = new Mock<IHostingEnvironment>(MockBehavior.Strict);
                mockHostingEnvironment.Setup(_ => _.WebRootPath).Returns(directory.Location).Verifiable();

                Directory.CreateDirectory(Path.Combine(directory.Location, "data"));
                File.WriteAllText(Path.Combine(directory.Location, @"data\BuildInfo.json"), JsonConvert.SerializeObject(buildInfo));
                File.WriteAllText(Path.Combine(directory.Location, @"manifest.json"), JsonConvert.SerializeObject(manifestNoHashes));

                var provider = new BuildInfoProvider(mockHostingEnvironment.Object);

                Assert.Equal("SomeBuildId", provider.BuildId);
                Assert.Equal("SomeChangelist", provider.Changelist);

                Assert.NotNull(provider.Webclient);
                Assert.Equal(manifestNoHashes.Count, provider.Webclient.Count);
                Assert.Equal(string.Empty, provider.Webclient["bundle0"]);
                Assert.Equal(string.Empty, provider.Webclient["bundle1"]);
                Assert.Equal(string.Empty, provider.Webclient["bundle2"]);

                mockHostingEnvironment.Verify();
            }
        }

        [Fact]
        public void BuildInfoProvider_WithHashes()
        {
            using (var directory = new TemporaryDirectory())
            {
                var mockHostingEnvironment = new Mock<IHostingEnvironment>(MockBehavior.Strict);
                mockHostingEnvironment.Setup(_ => _.WebRootPath).Returns(directory.Location).Verifiable();

                Directory.CreateDirectory(Path.Combine(directory.Location, "data"));
                File.WriteAllText(Path.Combine(directory.Location, @"data\BuildInfo.json"), JsonConvert.SerializeObject(buildInfo));
                File.WriteAllText(Path.Combine(directory.Location, @"manifest.json"), JsonConvert.SerializeObject(manifestWithHashes));

                var provider = new BuildInfoProvider(mockHostingEnvironment.Object);

                Assert.Equal("SomeBuildId", provider.BuildId);
                Assert.Equal("SomeChangelist", provider.Changelist);

                Assert.NotNull(provider.Webclient);
                Assert.Equal(manifestWithHashes.Count, provider.Webclient.Count);
                Assert.Equal("a", provider.Webclient["bundle0"]);
                Assert.Equal("b", provider.Webclient["bundle1"]);
                Assert.Equal("c", provider.Webclient["bundle2"]);

                mockHostingEnvironment.Verify();
            }
        }

        [Fact]
        public void BuildInfoProvider_EmptyBuildInfo()
        {
            using (var directory = new TemporaryDirectory())
            {
                var mockHostingEnvironment = new Mock<IHostingEnvironment>(MockBehavior.Strict);
                mockHostingEnvironment.Setup(_ => _.WebRootPath).Returns(directory.Location).Verifiable();

                Directory.CreateDirectory(Path.Combine(directory.Location, "data"));
                File.WriteAllText(Path.Combine(directory.Location, @"data\BuildInfo.json"), "{}");
                File.WriteAllText(Path.Combine(directory.Location, @"manifest.json"), JsonConvert.SerializeObject(manifestWithHashes));

                var provider = new BuildInfoProvider(mockHostingEnvironment.Object);

                Assert.Null(provider.BuildId);
                Assert.Null(provider.Changelist);

                Assert.NotNull(provider.Webclient);
                Assert.Equal(manifestWithHashes.Count, provider.Webclient.Count);
                Assert.Equal("a", provider.Webclient["bundle0"]);
                Assert.Equal("b", provider.Webclient["bundle1"]);
                Assert.Equal("c", provider.Webclient["bundle2"]);

                mockHostingEnvironment.Verify();
            }
        }

        [Fact]
        public void BuildInfoProvider_EmptyManifest()
        {
            using (var directory = new TemporaryDirectory())
            {
                var mockHostingEnvironment = new Mock<IHostingEnvironment>(MockBehavior.Strict);
                mockHostingEnvironment.Setup(_ => _.WebRootPath).Returns(directory.Location).Verifiable();

                Directory.CreateDirectory(Path.Combine(directory.Location, "data"));
                File.WriteAllText(Path.Combine(directory.Location, @"data\BuildInfo.json"), JsonConvert.SerializeObject(buildInfo));
                File.WriteAllText(Path.Combine(directory.Location, @"manifest.json"), "{}");

                var provider = new BuildInfoProvider(mockHostingEnvironment.Object);

                Assert.Equal("SomeBuildId", provider.BuildId);
                Assert.Equal("SomeChangelist", provider.Changelist);

                Assert.NotNull(provider.Webclient);
                Assert.Equal(0, provider.Webclient.Count);

                mockHostingEnvironment.Verify();
            }
        }

        [Fact]
        public void BuildInfoProvider_MissingBuildInfo()
        {
            using (var directory = new TemporaryDirectory())
            {
                var mockHostingEnvironment = new Mock<IHostingEnvironment>(MockBehavior.Strict);
                mockHostingEnvironment.Setup(_ => _.WebRootPath).Returns(directory.Location).Verifiable();

                File.WriteAllText(Path.Combine(directory.Location, @"manifest.json"), JsonConvert.SerializeObject(manifestWithHashes));

                Assert.Throws<InvalidDataException>(() => new BuildInfoProvider(mockHostingEnvironment.Object));

                mockHostingEnvironment.Verify();
            }
        }

        [Fact]
        public void BuildInfoProvider_MissingManifest()
        {
            using (var directory = new TemporaryDirectory())
            {
                var mockHostingEnvironment = new Mock<IHostingEnvironment>(MockBehavior.Strict);
                mockHostingEnvironment.Setup(_ => _.WebRootPath).Returns(directory.Location).Verifiable();

                Directory.CreateDirectory(Path.Combine(directory.Location, "data"));
                File.WriteAllText(Path.Combine(directory.Location, @"data\BuildInfo.json"), JsonConvert.SerializeObject(buildInfo));

                Assert.Throws<InvalidDataException>(() => new BuildInfoProvider(mockHostingEnvironment.Object));

                mockHostingEnvironment.Verify();
            }
        }
    }
}
