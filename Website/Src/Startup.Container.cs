// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Data.Tables;
using ClickerHeroesTrackerWebsite.Configuration;
using ClickerHeroesTrackerWebsite.Models;
using ClickerHeroesTrackerWebsite.Models.Game;
using ClickerHeroesTrackerWebsite.Models.Settings;
using ClickerHeroesTrackerWebsite.Services.Authentication;
using ClickerHeroesTrackerWebsite.Services.Database;
using ClickerHeroesTrackerWebsite.Services.Email;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;
using Website.Models.Authentication;
using Website.Services.Authentication;
using Website.Services.Clans;
using Website.Services.SiteNews;

namespace ClickerHeroesTrackerWebsite
{
    /// <summary>
    /// Configure the Unity container.
    /// </summary>
    public partial class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            if (_environment.IsDevelopment())
            {
                services.AddDatabaseDeveloperPageExceptionFilter();
            }

            // The DevelopmentStorageAccount will only work if you have the Storage emulator v4.3 installed: https://go.microsoft.com/fwlink/?linkid=717179&clcid=0x409
            string storageConnectionString = _configuration["Storage:ConnectionString"];
            TableServiceClient tableServiceClient = null;
            if (!string.IsNullOrEmpty(storageConnectionString))
            {
                tableServiceClient = new TableServiceClient(storageConnectionString);
            }

            // Necessary to persist keys (like the ones used to generate auth cookies)
            // By default Azure Websites can persist keys across instances within a slot, but not across slots.
            // This means a slot swap will require users to re-log in.
            // See https://github.com/aspnet/Home/issues/466 and https://github.com/aspnet/DataProtection/issues/92 for details.
            IDataProtectionBuilder dataProtectionBuilder = services.AddDataProtection();
            if (!string.IsNullOrEmpty(storageConnectionString))
            {
                dataProtectionBuilder.PersistKeysToAzureBlobStorage(storageConnectionString, "key-container", "keys.xml");
            }

            // Add Entity framework services.
            services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseSqlServer(_configuration["Database:ConnectionString"]);

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
                    options.ClaimsIdentity.UserNameClaimType = OpenIddictConstants.Claims.Name;
                    options.ClaimsIdentity.UserIdClaimType = OpenIddictConstants.Claims.Subject;
                    options.ClaimsIdentity.EmailClaimType = OpenIddictConstants.Claims.Email;
                    options.ClaimsIdentity.RoleClaimType = OpenIddictConstants.Claims.Role;
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
                    // Enable the token endpoint (required to use the password flow).
                    options.SetTokenEndpointUris("api/auth/token");

                    // Allow client applications to use the grant_type=password flow.
                    options.AllowPasswordFlow()
                        .AllowRefreshTokenFlow()
                        .AllowCustomFlow(GoogleAssertionGrantHandler.GrantType)
                        .AllowCustomFlow(FacebookAssertionGrantHandler.GrantType)
                        .AllowCustomFlow(MicrosoftAssertionGrantHandler.GrantType);

                    // Mark the "email", "profile" and "roles" scopes as supported scopes.
                    options.RegisterScopes(
                        OpenIddictConstants.Scopes.Email,
                        OpenIddictConstants.Scopes.Profile,
                        OpenIddictConstants.Scopes.Roles);

                    // Accept anonymous clients (i.e clients that don't send a client_id).
                    options.AcceptAnonymousClients();

                    // Use ASP.NET Core data protection
                    options.UseDataProtection();

                    // Register the signing and encryption credentials.
                    // Use ephemeral keys since we're using Data Protection to validate and issue tokens anyway,
                    // but OpenIddict doesn't allow us to not register anything.
                    options.AddEphemeralEncryptionKey()
                           .AddEphemeralSigningKey();

                    // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
                    options.UseAspNetCore()
                           .EnableTokenEndpointPassthrough();
                })

                // Register the OpenIddict validation components.
                .AddValidation(options =>
                {
                    // Import the configuration from the local OpenIddict server instance.
                    options.UseLocalServer();

                    // Register the ASP.NET Core host.
                    options.UseAspNetCore();

                    // Use ASP.NET Core data protection
                    options.UseDataProtection();
                });

            services.Configure((AssertionGrantOptions options) =>
            {
                options.AddAssertionGrantType<GoogleAssertionGrantHandler>(GoogleAssertionGrantHandler.GrantType);
                options.AddAssertionGrantType<FacebookAssertionGrantHandler>(FacebookAssertionGrantHandler.GrantType);
                options.AddAssertionGrantType<MicrosoftAssertionGrantHandler>(MicrosoftAssertionGrantHandler.GrantType);
            });

            services.AddSingleton<GoogleAssertionGrantHandler>();
            services.AddSingleton<FacebookAssertionGrantHandler>();
            services.AddSingleton<MicrosoftAssertionGrantHandler>();

            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .Build();
            });

            IBuildInfoProvider buildInfoProvider = _environment.IsDevelopment()
                ? new DeveloperBuildInfoProvider()
                : new FileBuildInfoProvider(Path.Combine(_environment.ContentRootPath, "BuildInfo.json"));

            services.AddApplicationInsightsTelemetry(new ApplicationInsightsServiceOptions
            {
                ApplicationVersion = buildInfoProvider.BuildUrl,
                DeveloperMode = _environment.IsDevelopment(),
            });

            services.AddCors(options =>
            {
                // Allow WebClient dev-server to call the API locally.
                if (_environment.IsDevelopment())
                {
                    options.AddDefaultPolicy(builder =>
                    {
                        builder
                            .WithOrigins("http://localhost:4200")
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials();
                    });
                }
            });

            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    // Beautify by default for debuggability. When gzipping, this barely adds anything to the payload.
                    options.JsonSerializerOptions.WriteIndented = true;

                    // Omit nulls
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

                    // Use camel-casing for fields (lower case first character)
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

                    // Convert enum values to strings
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
                });

            // Allow IOptions<T> to be available through DI
            services.AddOptions();

            // Container controlled registrations
            if (tableServiceClient != null)
            {
                services.AddSingleton(tableServiceClient);
                services.AddSingleton<ISiteNewsProvider, AzureStorageSiteNewsProvider>();
            }
            else
            {
                services.AddSingleton<ISiteNewsProvider, InMemorySiteNewsProvider>();
            }

            string gameDataDirectoryPath = _environment.IsDevelopment()
                ? Path.Combine(_environment.ContentRootPath, @"..\..")
                : _environment.ContentRootPath;

            services.AddSingleton(_ => GameData.Parse(Path.Combine(gameDataDirectoryPath, "GameData.json")));
            services.AddSingleton(_ => new HttpClient());
            services.AddSingleton<IAssertionGrantHandlerProvider, AssertionGrantHandlerProvider>();
            services.AddSingleton(buildInfoProvider);
            services.AddSingleton<IEmailSender, EmailSender>();
            services.AddSingleton<IOptions<PasswordHasherOptions>, PasswordHasherOptionsAccessor>();

            // Per request registrations
            services.AddScoped<IClanManager, ClanManager>();
            services.AddScoped<IDatabaseCommandFactory, DatabaseCommandFactory>();
            services.AddScoped<IUserSettingsProvider, UserSettingsProvider>();

            // configuration
            services.Configure<AuthenticationSettings>(options => _configuration.GetSection("Authentication").Bind(options));
            services.Configure<DatabaseSettings>(options => _configuration.GetSection("Database").Bind(options));
            services.Configure<EmailSenderSettings>(options => _configuration.GetSection("EmailSender").Bind(options));
        }
    }
}
