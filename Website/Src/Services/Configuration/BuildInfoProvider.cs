// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ClickerHeroesTrackerWebsite.Configuration
{
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
            string buildInfoFile = Path.Combine(webHostEnvironment.WebRootPath, "data", "BuildInfo.json");
            if (!File.Exists(buildInfoFile))
            {
                throw new InvalidDataException("Could not find build info file: " + buildInfoFile);
            }

            // PreBuild.ps1 writes over this file during cloud build.
            using (JsonTextReader reader = new(new StreamReader(buildInfoFile)))
            {
                JObject buildInfo = JObject.Load(reader);

                Changelist = (string)buildInfo["changelist"];
                BuildUrl = (string)buildInfo["buildUrl"];
            }

            string webclientManifestFile = Path.Combine(webHostEnvironment.WebRootPath, @"manifest.json");
            if (!File.Exists(webclientManifestFile))
            {
                throw new InvalidDataException("Could not find Webclient manifest file: " + webclientManifestFile);
            }

            Dictionary<string, string> webClient = new(StringComparer.OrdinalIgnoreCase);
            using (JsonTextReader reader = new(new StreamReader(webclientManifestFile)))
            {
                JsonSerializer serializer = new();
                Dictionary<string, string> manifest = serializer.Deserialize<Dictionary<string, string>>(reader);

                foreach (KeyValuePair<string, string> pair in manifest)
                {
                    string name = pair.Key;
                    string file = pair.Value;

                    // We only care about the js files. Ignore things like the source maps and index.html
                    if (!name.EndsWith(".js", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    // each entry looks like { "app.js", "/d8df654c29474da7b106.js" }, but we want them to look like { "app": "d8df654c29474da7b106" }.
                    string bundleName = Path.GetFileNameWithoutExtension(name);
                    string hash = file.Substring(1, file.Length - 4);

                    webClient.Add(bundleName, hash);
                }
            }

            Webclient = webClient;
        }

        /// <inheritdoc/>
        public string Changelist { get; }

        /// <inheritdoc/>
        public string BuildUrl { get; }

        public IDictionary<string, string> Webclient { get; }
    }
}