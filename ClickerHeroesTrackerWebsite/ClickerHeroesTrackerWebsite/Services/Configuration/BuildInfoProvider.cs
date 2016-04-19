// <copyright file="EnvironmentProvider.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Configuration
{
    using System.IO;
    using Microsoft.AspNet.Hosting;
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
            var buildInfoFile = hostingEnvironment.MapPath(@"data\BuildInfo.json");
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
        }

        /// <inheritdoc/>
        public string Changelist { get; }

        /// <inheritdoc/>
        public string BuildId { get; }
    }
}