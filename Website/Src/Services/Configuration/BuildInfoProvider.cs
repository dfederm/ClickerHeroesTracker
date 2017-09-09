// <copyright file="BuildInfoProvider.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.AspNetCore.Hosting;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Basic implementation for retrieving the environment information from application settings, the build info files, and other sources.
    /// </summary>
    public sealed class BuildInfoProvider : IBuildInfoProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BuildInfoProvider"/> class.
        /// </summary>
        public BuildInfoProvider(IHostingEnvironment hostingEnvironment)
        {
            var buildInfoFile = Path.Combine(hostingEnvironment.WebRootPath, @"data\BuildInfo.json");
            if (!File.Exists(buildInfoFile))
            {
                throw new InvalidDataException("Could not find build info file: " + buildInfoFile);
            }

            // PreBuild.ps1 writes over this file during cloud build.
            using (var reader = new JsonTextReader(new StreamReader(buildInfoFile)))
            {
                var buildInfo = JObject.Load(reader);

                this.Changelist = (string)buildInfo["changelist"];
                this.BuildId = (string)buildInfo["buildId"];
            }

            var webclientManifestFile = Path.Combine(hostingEnvironment.WebRootPath, @"manifest.json");
            if (!File.Exists(webclientManifestFile))
            {
                throw new InvalidDataException("Could not find Webclient manifest file: " + webclientManifestFile);
            }

            var webClient = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            using (var reader = new JsonTextReader(new StreamReader(webclientManifestFile)))
            {
                var serializer = new JsonSerializer();
                var manifest = serializer.Deserialize<Dictionary<string, string>>(reader);

                foreach (var pair in manifest)
                {
                    var name = pair.Key;
                    var file = pair.Value;

                    if (name.EndsWith(".map", StringComparison.OrdinalIgnoreCase))
                    {
                        // We don't care about the map files
                        continue;
                    }

                    // each entry looks like "app.js": "app.5efeb981068fadf86ea1.js", but we want them to look like "app": "5efeb981068fadf86ea1".
                    var bundleName = Path.GetFileNameWithoutExtension(name);
                    var hashLength = file.Length - name.Length - 1;
                    var hash = hashLength > 0
                        ? file.Substring(bundleName.Length + 1, hashLength)
                        : string.Empty;

                    webClient.Add(bundleName, hash);
                }
            }

            this.Webclient = webClient;
        }

        /// <inheritdoc/>
        public string Changelist { get; }

        /// <inheritdoc/>
        public string BuildId { get; }

        public IDictionary<string, string> Webclient { get; }
    }
}