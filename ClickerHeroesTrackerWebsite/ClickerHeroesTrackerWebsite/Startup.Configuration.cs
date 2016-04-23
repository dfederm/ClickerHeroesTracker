// <copyright file="Startup.Configuration.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite
{
    using AzureAppService.Configuration;
    using Microsoft.AspNet.Hosting;
    using Microsoft.Extensions.Configuration;

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
                .AddJsonFile("appsettings.json")
                .AddAzureAppServiceSettings();

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets();
            }

            builder.AddEnvironmentVariables();
            this.Configuration = builder.Build();
        }
    }
}