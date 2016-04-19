// <copyright file="Startup.Configuration.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite
{
    using Microsoft.AspNet.Hosting;
    using Microsoft.Extensions.Configuration;
    using System.Collections.Generic;
    using System.Configuration;

    /// <summary>
    /// Configure the Unity container
    /// </summary>
    public partial class Startup
    {
        public IConfiguration Configuration { get; set; }

        public IHostingEnvironment Environment { get; set; }

        public Startup(IHostingEnvironment env)
        {
            this.Environment = env;

            // Set up configuration sources.
            var builder = new ConfigurationBuilder()
                .AddInMemoryCollection(GetAppConfig())
                .AddJsonFile("appsettings.json");

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets();
            }

            builder.AddEnvironmentVariables();
            this.Configuration = builder.Build();
        }

        private static Dictionary<string, string> GetAppConfig()
        {
            var data = new Dictionary<string, string>();

            var appSettings = ConfigurationManager.AppSettings;
            foreach (string appSetting in appSettings.Keys)
            {
                data[appSetting] = appSettings[appSetting];
            }

            var connectionStrings = ConfigurationManager.ConnectionStrings;
            foreach (ConnectionStringSettings connectionString in connectionStrings)
            {
                data[connectionString.Name] = connectionString.ConnectionString;
            }

            return data;
        }
    }
}