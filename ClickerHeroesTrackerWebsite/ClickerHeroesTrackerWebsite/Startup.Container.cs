// <copyright file="Startup.Container.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite
{
    using System;
    using System.IO;
    using System.Linq;
    using ClickerHeroesTrackerWebsite.Configuration;
    using ClickerHeroesTrackerWebsite.Instrumentation;
    using ClickerHeroesTrackerWebsite.Models;
    using ClickerHeroesTrackerWebsite.Models.Game;
    using ClickerHeroesTrackerWebsite.Models.Settings;
    using ClickerHeroesTrackerWebsite.Services.Authentication;
    using ClickerHeroesTrackerWebsite.Services.ContentManagement;
    using ClickerHeroesTrackerWebsite.Services.Database;
    using ClickerHeroesTrackerWebsite.Services.Email;
    using ClickerHeroesTrackerWebsite.Services.UploadProcessing;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.AspNetCore.Mvc.Formatters;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Microsoft.Net.Http.Headers;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Queue;
    using Microsoft.WindowsAzure.Storage.Table;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// Configure the Unity container
    /// </summary>
    public partial class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // The DevelopmentStorageAccount will only work if you have the Storage emulator v4.3 installed: https://go.microsoft.com/fwlink/?linkid=717179&clcid=0x409
            bool useDevelopmentStorageAccount;
            var storageConnectionString = this.Configuration["Storage:ConnectionString"];
            var storageAccount = string.IsNullOrEmpty(storageConnectionString)
                ? bool.TryParse(this.Configuration["Storage:UseDevelopmentStorageAccount"], out useDevelopmentStorageAccount) && useDevelopmentStorageAccount
                    ? CloudStorageAccount.DevelopmentStorageAccount
                    : null
                : CloudStorageAccount.Parse(storageConnectionString);

            // Nesessary to persist keys (like the ones used to generate auth cookies)
            // By default Azure Websites can persist keys across instances within a slot, but not across slots.
            // This means a slot swap will require users to re-log in.
            // See https://github.com/aspnet/Home/issues/466 and https://github.com/aspnet/DataProtection/issues/92 for details.
            var dataProtectionBuilder = services.AddDataProtection();
            if (storageAccount != null)
            {
                var client = storageAccount.CreateCloudBlobClient();
                var container = client.GetContainerReference("key-container");

                // The container must exist before calling the DataProtection APIs.
                // The specific file within the container does not have to exist,
                // as it will be created on-demand.
                container.CreateIfNotExistsAsync().Wait();

                dataProtectionBuilder.PersistKeysToAzureBlobStorage(container, "keys.xml");
            }

            // Add Entity framework services.
            var connectionString = this.Configuration["Database:ConnectionString"];
            switch (this.Configuration["Database:Kind"])
            {
                case "SqlServer":
                {
                    services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
                    break;
                }
                case "Sqlite":
                {
                    services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connectionString));
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
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Register the OpenIddict services, including the default Entity Framework stores.
            var openIddictBuilder = services.AddOpenIddict<ApplicationUser, ApplicationDbContext>()
                // Enable the token endpoint (required to use the password flow).
                .EnableTokenEndpoint("/api/connect/token")

                // Allow client applications to use the grant_type=password flow.
                .AllowPasswordFlow();

            // Allow Http on devbox
            if (this.Environment.IsDevelopment())
            {
                openIddictBuilder.DisableHttpsRequirement();
            }

            services.AddApplicationInsightsTelemetry(this.Configuration);

            services.AddMvc(options =>
            {
                options.Filters.Add(typeof(MeasureLatencyFilter));

                var jsonFormatter = options.OutputFormatters.OfType<JsonOutputFormatter>().Single();

                // Allow the json formatter to handle requests from the browser
                jsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
            }).AddJsonOptions(options =>
            {
                // Beautify by default for debuggability. When gzipping, this barely adds anything to the payload.
                options.SerializerSettings.Formatting = Formatting.Indented;

                // Omit nulls
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;

                // Use camel-casing for fields (lower case first character)
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

                // Convert enum values to strings
                options.SerializerSettings.Converters.Add(new StringEnumConverter { CamelCaseText = true });
            });

            // Allow IOptions<T> to be available through DI
            services.AddOptions();

            // Container controlled registrations
            services.AddSingleton<CloudStorageAccount>(_ => storageAccount);
            services.AddSingleton<CloudTableClient>(_ => _.GetService<CloudStorageAccount>().CreateCloudTableClient());
            services.AddSingleton<CloudQueueClient>(_ => _.GetService<CloudStorageAccount>().CreateCloudQueueClient());
            services.AddSingleton<GameData>(_ => GameData.Parse(Path.Combine(this.Environment.ContentRootPath, @"data\GameData.json")));
            services.AddSingleton<IBuildInfoProvider, BuildInfoProvider>();
            services.AddSingleton<IEmailSender, EmailSender>();
            services.AddSingleton<IOptions<PasswordHasherOptions>, PasswordHasherOptionsAccessor>();
            services.AddSingleton<IUploadScheduler, UploadScheduler>();

            // Per request registrations
            services.AddScoped<IContentManager, ContentManager>();
            services.AddScoped<ICounterProvider, CounterProvider>();
            services.AddScoped<IDatabaseCommandFactory, DatabaseCommandFactory>();
            services.AddScoped<IUserSettingsProvider, UserSettingsProvider>();

            // Configuration
            services.Configure<AuthenticationSettings>(options => this.Configuration.GetSection("Authentication").Bind(options));
            services.Configure<DatabaseSettings>(options => this.Configuration.GetSection("Database").Bind(options));
            services.Configure<EmailSenderSettings>(options => this.Configuration.GetSection("EmailSender").Bind(options));
        }
    }
}