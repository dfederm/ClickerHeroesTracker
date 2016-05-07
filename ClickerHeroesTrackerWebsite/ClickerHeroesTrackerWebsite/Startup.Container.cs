// <copyright file="Startup.Container.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite
{
    using Configuration;
    using Database;
    using Instrumentation;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;
    using Models.Game;
    using Models.Settings;
    using UploadProcessing;
    using Microsoft.AspNet.Hosting;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Microsoft.Data.Entity;
    using Microsoft.Extensions.DependencyInjection;
    using ClickerHeroesTrackerWebsite.Models;
    using ClickerHeroesTrackerWebsite.Services;
    using System;
    using System.Linq;
    using Microsoft.AspNet.Mvc.Formatters;
    using Microsoft.Net.Http.Headers;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using Newtonsoft.Json.Converters;
    using Services.ContentManagement;
    using Microsoft.Extensions.Configuration;
    using Services.Authentication;
    using Microsoft.AspNet.DataProtection;
    using Microsoft.AspNet.Identity;
    using Microsoft.Extensions.OptionsModel;
    
    /// <summary>
    /// Configure the Unity container
    /// </summary>
    public partial class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // The DevelopmentStorageAccount will only work if you have the Storage emulator v4.3 installed: https://go.microsoft.com/fwlink/?linkid=717179&clcid=0x409
            var storageConnectionString = this.Configuration["Storage:ConnectionString"];
            var storageAccount = string.IsNullOrEmpty(storageConnectionString)
                ? this.Environment.IsDevelopment()
                    ? CloudStorageAccount.DevelopmentStorageAccount
                    : null
                : CloudStorageAccount.Parse(storageConnectionString);

            // Nesessary to persist keys (like the ones used to generate auth cookies)
            // By default Azure Websites can persist keys across instances within a slot, but not across slots.
            // This means a slot swap will require users to re-log in.
            // See https://github.com/aspnet/Home/issues/466 and https://github.com/aspnet/DataProtection/issues/92 for details.
            services.AddDataProtection();
            services.ConfigureDataProtection(options =>
            {
                if (storageAccount != null)
                {
                    var client = storageAccount.CreateCloudBlobClient();
                    var container = client.GetContainerReference("key-container");

                    // The container must exist before calling the DataProtection APIs.
                    // The specific file within the container does not have to exist,
                    // as it will be created on-demand.
                    container.CreateIfNotExists();

                    options.PersistKeysToAzureBlobStorage(container, "keys.xml");
                }
            });

            // Add Entity framework services.
            var entityFramework = services.AddEntityFramework();
            var connectionString = this.Configuration["Database:ConnectionString"];
            switch (this.Configuration["Database:Kind"])
            {
                case "SqlServer":
                {
                    entityFramework
                        .AddSqlServer()
                        .AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
                    break;
                }
                case "Sqlite":
                {
                    entityFramework
                        .AddSqlite()
                        .AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connectionString));
                    break;
                }
                default:
                {
                    throw new InvalidOperationException($"Invalid configuration for \"Database:Kind\": {this.Configuration["Database:Kind"]}");
                }
            }

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
                {
                    // We need to disallow '@' since we need to disambiguate between user names and email addresses during log in
                    options.User.AllowedUserNameCharacters = options.User.AllowedUserNameCharacters.Replace("@", string.Empty);

                    options.User.RequireUniqueEmail = true;

                    options.Password.RequiredLength = 4;
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireNonLetterOrDigit = false;
                    options.Password.RequireUppercase = false;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddApplicationInsightsTelemetry(this.Configuration);

            services.AddMvc(options =>
            {
                options.Filters.Add(typeof(MeasureLatencyFilter));

                var jsonFormatter = options.OutputFormatters.OfType<JsonOutputFormatter>().Single();

                // Allow the json formatter to handle requests from the browser
                jsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));

                // Beautify by default for debuggability. When gzipping, this barely adds anything to the payload.
                jsonFormatter.SerializerSettings.Formatting = Formatting.Indented;

                // Omit nulls
                jsonFormatter.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;

                // Use camel-casing for fields (lower case first character)
                jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

                // Convert enum values to strings
                jsonFormatter.SerializerSettings.Converters.Add(new StringEnumConverter { CamelCaseText = true });

            });

            services.AddInstance<IConfiguration>(this.Configuration);

            // Container controlled registrations
            services.AddSingleton<CloudStorageAccount>(_ => storageAccount);
            services.AddSingleton<CloudTableClient>(_ => _.GetService<CloudStorageAccount>().CreateCloudTableClient());
            services.AddSingleton<GameData>(_ => GameData.Parse(this.Environment.MapPath(@"data\GameData.json")));
            services.AddSingleton<IBuildInfoProvider, BuildInfoProvider>();
            services.AddSingleton<IEmailSender, EmailSender>();
            services.AddSingleton<IOptions<PasswordHasherOptions>, PasswordHasherOptionsAccessor>();
            services.AddSingleton<IUploadScheduler, UploadScheduler>();

            // Per request registrations
            services.AddScoped<IContentManager, ContentManager>();
            services.AddScoped<ICounterProvider, CounterProvider>();
            services.AddScoped<IDatabaseCommandFactory, DatabaseCommandFactory>();
            services.AddScoped<IUserSettingsProvider, UserSettingsProvider>();
        }
    }
}