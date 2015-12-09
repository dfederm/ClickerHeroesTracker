// <copyright file="EnvironmentProvider.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Configuration
{
    using System.Configuration;
    using System.IO;
    using System.Web.Hosting;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Basic implementation for retrieving the environment information from application settings, the build info files, and other sources.
    /// </summary>
    public sealed class EnvironmentProvider : IEnvironmentProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnvironmentProvider"/> class.
        /// </summary>
        public EnvironmentProvider()
        {
            // This is a slot setting set in the Azure portal.
            this.Environment = ConfigurationManager.AppSettings.Get("Environment") ?? "Devmachine";

            var buildInfoFile = HostingEnvironment.MapPath(@"~\App_Data\BuildInfo.json");
            if (!File.Exists(buildInfoFile))
            {
                throw new InvalidDataException("Could not find build info file: " + buildInfoFile);
            }

            // PreBuild.ps1 writes over this file during cloud build.
            using (var reader = new JsonTextReader(new StreamReader(buildInfoFile)))
            {
                var buildInfo = JObject.Load(reader);

                this.Changelist = (int)buildInfo["changelist"];
                this.BuildId = (string)buildInfo["buildId"];
            }
        }

        /// <inheritdoc />
        public string Environment { get; }

        /// <inheritdoc/>
        public int Changelist { get; }

        /// <inheritdoc/>
        public string BuildId { get; }
    }
}