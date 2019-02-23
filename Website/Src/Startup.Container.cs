// <copyright file="Startup.Container.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using AspNet.Security.OAuth.Validation;
    using AspNet.Security.OpenIdConnect.Primitives;
    using ClickerHeroesTrackerWebsite.Configuration;
    using ClickerHeroesTrackerWebsite.Models;
    using ClickerHeroesTrackerWebsite.Models.Game;
    using ClickerHeroesTrackerWebsite.Models.Settings;
    using ClickerHeroesTrackerWebsite.Services.Authentication;
    using ClickerHeroesTrackerWebsite.Services.Database;
    using ClickerHeroesTrackerWebsite.Services.Email;
    using ClickerHeroesTrackerWebsite.Services.UploadProcessing;
    using Microsoft.ApplicationInsights.AspNetCore.Extensions;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Identity;
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
    using OpenIddict.Abstractions;
    using Website.Models.Authentication;
    using Website.Services.Authentication;
    using Website.Services.Clans;
    using Website.Services.SiteNews;

    /// <summary>
    /// Configure the Unity container.
    /// </summary>
    public partial class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // The DevelopmentStorageAccount will only work if you have the Storage emulator v4.3 installed: https://go.microsoft.com/fwlink/?linkid=717179&clcid=0x409
            var storageConnectionString = this.configuration["Storage:ConnectionString"];
            var storageAccount = !string.IsNullOrEmpty(storageConnectionString)
                ? CloudStorageAccount.Parse(storageConnectionString)
                : null;

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
            services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseSqlServer(this.configuration["Database:ConnectionString"]);

                    // Register the entity sets needed by OpenIddict.
                    options.UseOpenIddict();
                });

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
                {
                    // We need to disallow '@' since we need to disambiguate between user names and email addresses during log in
                    options.User.AllowedUserNameCharacters = options.User.AllowedUserNameCharacters.Replace("@", string.Empty, StringComparison.Ordinal);

                    options.User.RequireUniqueEmail = true;

                    options.Password.RequiredLength = 4;
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;

                    // Configure Identity to use the same JWT claims as OpenIddict instead
                    // of the legacy WS-Federation claims it uses by default (ClaimTypes),
                    // which saves you from doing the mapping in your authorization controller.
                    options.ClaimsIdentity.UserNameClaimType = OpenIdConnectConstants.Claims.Name;
                    options.ClaimsIdentity.UserIdClaimType = OpenIdConnectConstants.Claims.Subject;
                    options.ClaimsIdentity.RoleClaimType = OpenIdConnectConstants.Claims.Role;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Register the OpenIddict services.
            services.AddOpenIddict()

                // Register the OpenIddict core services.
                .AddCore(options =>
                {
                    // Configure OpenIddict to use the Entity Framework Core stores and models.
                    options.UseEntityFrameworkCore()
                        .UseDbContext<ApplicationDbContext>();
                })

                // Register the OpenIddict server handler.
                .AddServer(options =>
                {
                    // Register the ASP.NET Core MVC services used by OpenIddict.
                    // Note: if you don't call this method, you won't be able to
                    // bind OpenIdConnectRequest or OpenIdConnectResponse parameters.
                    options.UseMvc();

                    // Enable the token endpoint (required to use the password flow).
                    options.EnableTokenEndpoint("/api/auth/token");

                    // Allow client applications to use the grant_type=password flow.
                    options.AllowPasswordFlow()
                        .AllowRefreshTokenFlow()
                        .AllowCustomFlow(GoogleAssertionGrantHandler.GrantType)
                        .AllowCustomFlow(FacebookAssertionGrantHandler.GrantType)
                        .AllowCustomFlow(MicrosoftAssertionGrantHandler.GrantType);

                    // Mark the "email", "profile" and "roles" scopes as supported scopes.
                    options.RegisterScopes(
                        OpenIdConnectConstants.Scopes.Email,
                        OpenIdConnectConstants.Scopes.Profile,
                        OpenIddictConstants.Scopes.Roles);

                    // When request caching is enabled, authorization and logout requests
                    // are stored in the distributed cache by OpenIddict and the user agent
                    // is redirected to the same page with a single parameter (request_id).
                    // This allows flowing large OpenID Connect requests even when using
                    // an external authentication provider like Google, Facebook or Twitter.
                    options.EnableRequestCaching();

                    // We don't want to specify a client_id when sending a token or revocation request.
                    options.AcceptAnonymousClients();

                    // When rolling tokens are enabled, immediately
                    // redeem the refresh token to prevent future reuse.
                    options.UseRollingTokens();
                })

                // Register the OpenIddict validation handler.
                // Note: the OpenIddict validation handler is only compatible with the
                // default token format or with reference tokens and cannot be used with
                // JWT tokens. For JWT tokens, use the Microsoft JWT bearer handler.
                .AddValidation();

            services.Configure((AssertionGrantOptions options) =>
            {
                options.AddAssertionGrantType<GoogleAssertionGrantHandler>(GoogleAssertionGrantHandler.GrantType);
                options.AddAssertionGrantType<FacebookAssertionGrantHandler>(FacebookAssertionGrantHandler.GrantType);
                options.AddAssertionGrantType<MicrosoftAssertionGrantHandler>(MicrosoftAssertionGrantHandler.GrantType);
            });

            services.AddSingleton<GoogleAssertionGrantHandler>();
            services.AddSingleton<FacebookAssertionGrantHandler>();
            services.AddSingleton<MicrosoftAssertionGrantHandler>();

            services.AddAuthentication();

            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(OAuthValidationDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .Build();
            });

            var buildInfoProvider = new BuildInfoProvider(this.environment);

            services.Configure((ApplicationInsightsServiceOptions options) =>
            {
                options.ApplicationVersion = buildInfoProvider.BuildId;
                options.DeveloperMode = this.environment.IsDevelopment();
            });

            services.AddCors();

            services.AddMvc(options =>
            {
                ////var jsonFormatter = options.OutputFormatters.OfType<NewtonsoftJsonOutputFormatter>().Single();

                // Allow the json formatter to handle requests from the browser
                ////jsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
            }).AddNewtonsoftJson(options =>
            {
                // Beautify by default for debuggability. When gzipping, this barely adds anything to the payload.
                options.SerializerSettings.Formatting = Formatting.Indented;

                // Omit nulls
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;

                // Use camel-casing for fields (lower case first character)
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

                // Convert enum values to strings
                options.SerializerSettings.Converters.Add(new StringEnumConverter { NamingStrategy = new CamelCaseNamingStrategy() });
            });

            // Allow IOptions<T> to be available through DI
            services.AddOptions();

            // Container controlled registrations
            if (storageAccount != null)
            {
                services.AddSingleton<CloudTableClient>(_ => storageAccount.CreateCloudTableClient());
                services.AddSingleton<CloudQueueClient>(_ => storageAccount.CreateCloudQueueClient());
                services.AddSingleton<IUploadScheduler, AzureStorageUploadScheduler>();
                services.AddSingleton<ISiteNewsProvider, AzureStorageSiteNewsProvider>();
            }
            else
            {
                services.AddSingleton<IUploadScheduler, NoOpUploadScheduler>();
                services.AddSingleton<ISiteNewsProvider, InMemorySiteNewsProvider>();
            }

            services.AddSingleton<GameData>(_ => GameData.Parse(Path.Combine(this.environment.WebRootPath, @"data\GameData.json")));
            services.AddSingleton<HttpClient>(_ => new HttpClient());
            services.AddSingleton<IAssertionGrantHandlerProvider, AssertionGrantHandlerProvider>();
            services.AddSingleton<IBuildInfoProvider>(buildInfoProvider);
            services.AddSingleton<IEmailSender, EmailSender>();
            services.AddSingleton<IOptions<PasswordHasherOptions>, PasswordHasherOptionsAccessor>();

            // Per request registrations
            services.AddScoped<IClanManager, ClanManager>();
            services.AddScoped<IDatabaseCommandFactory, DatabaseCommandFactory>();
            services.AddScoped<IUserSettingsProvider, UserSettingsProvider>();

            // configuration
            services.Configure<AuthenticationSettings>(options => this.configuration.GetSection("Authentication").Bind(options));
            services.Configure<DatabaseSettings>(options => this.configuration.GetSection("Database").Bind(options));
            services.Configure<EmailSenderSettings>(options => this.configuration.GetSection("EmailSender").Bind(options));
        }
    }
}