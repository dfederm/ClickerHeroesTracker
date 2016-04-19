// <copyright file="Startup.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite
{
    using ClickerHeroesTrackerWebsite.Authentication;
    using ClickerHeroesTrackerWebsite.Instrumentation;
    using ClickerHeroesTrackerWebsite.Models.Game;
    using ClickerHeroesTrackerWebsite.Models.Settings;
    using Microsoft.AspNet.Builder;
    using Microsoft.AspNet.Hosting;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Models;
    using Utility;

    public partial class Startup
    {
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();
            loggerFactory.AddDebug();

            app.UseApplicationInsightsRequestTelemetry();
            app.UseApplicationInsightsExceptionTelemetry();

            if (env.IsDevelopment() || this.Environment.IsBuddy())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();

                // Only allow telemetry in production
                app.ApplicationServices.GetService<TelemetryConfiguration>().DisableTelemetry = true;
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            // We want to start measuring latency as soon as possible during a request.
            app.UseMiddleware<MeasureLatencyMiddleware>();

            app.UseIISPlatformHandler(options => options.AuthenticationDescriptions.Clear());

            app.UseStaticFiles();

            app.UseIdentity();

            // To configure external authentication please see http://go.microsoft.com/fwlink/?LinkID=532715
            var microsoftClientId = Configuration["Authentication:Microsoft:ClientId"];
            var microsoftClientSecret = Configuration["Authentication:Microsoft:ClientSecret"];
            if (!string.IsNullOrEmpty(microsoftClientId) && !string.IsNullOrEmpty(microsoftClientSecret))
            {
                app.UseMicrosoftAccountAuthentication(options =>
                {
                    options.ClientId = microsoftClientId;
                    options.ClientSecret = microsoftClientSecret;
                });
            }

            // Allow auth mocking when not in prod
            if (!env.IsProduction())
            {
                app.UseMiddleware<MockAuthenticationOwinMiddleware>();
            }

            // Instrument the user as soon as they're auth'd.
            app.UseMiddleware<UserInstrumentationMiddleware>();

            // Flush any changes to user settings
            app.UseMiddleware<UserSettingsFlushingMiddleware>();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}");
            });

            // Ensure the database is created. This is mostly for the in-memory database used in dev mode, but could be useful for new databases too.
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetService<ApplicationDbContext>().Database.EnsureCreated();
            }

            // Warm up the game data parsing
            app.ApplicationServices.GetService<GameData>();
        }

        // Entry point for the application.
        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}
