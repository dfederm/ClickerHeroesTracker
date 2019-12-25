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
        public BuildInfoProvider(IWebHostEnvironment webHostEnvironment)
        {
            var buildInfoFile = Path.Combine(webHostEnvironment.WebRootPath, @"data\BuildInfo.json");
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

            var webclientManifestFile = Path.Combine(webHostEnvironment.WebRootPath, @"manifest.json");
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

                    // We only care about the js files. Ignore things like the source maps and index.html
                    if (!name.EndsWith(".js", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    // each entry looks like { "app.js", "/d8df654c29474da7b106.js" }, but we want them to look like { "app": "d8df654c29474da7b106" }.
                    var bundleName = Path.GetFileNameWithoutExtension(name);
                    var hash = file.Substring(1, file.Length - 4);

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