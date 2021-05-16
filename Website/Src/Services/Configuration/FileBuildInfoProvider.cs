// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ClickerHeroesTrackerWebsite.Configuration
{
    /// <summary>
    /// Implementation of <see cref="IBuildInfoProvider" /> for running in the cloud. This implementation retrieves the environment information from application settings, the build info files, and other sources.
    /// </summary>
    public sealed class FileBuildInfoProvider : IBuildInfoProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileBuildInfoProvider"/> class.
        /// </summary>
        public FileBuildInfoProvider(string buildInfoFile)
        {
            if (!File.Exists(buildInfoFile))
            {
                throw new InvalidDataException("Could not find build info file: " + buildInfoFile);
            }

            using (JsonTextReader reader = new(new StreamReader(buildInfoFile)))
            {
                JObject buildInfo = JObject.Load(reader);

                Changelist = buildInfo["changelist"]?.Value<string>();
                BuildUrl = buildInfo["buildUrl"]?.Value<string>();
                Webclient = buildInfo["webclient"]?.ToObject<Dictionary<string, string>>();
            }
        }

        /// <inheritdoc/>
        public string Changelist { get; }

        /// <inheritdoc/>
        public string BuildUrl { get; }

        public IDictionary<string, string> Webclient { get; }
    }
}