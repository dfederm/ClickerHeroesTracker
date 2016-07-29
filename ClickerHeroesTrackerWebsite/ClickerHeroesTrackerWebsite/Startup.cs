// <copyright file="Startup.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite
{
    using ClickerHeroesTrackerWebsite.Instrumentation;
    using ClickerHeroesTrackerWebsite.Models.Game;
    using ClickerHeroesTrackerWebsite.Models.Settings;
    using ClickerHeroesTrackerWebsite.Services.Security;
    using ClickerHeroesTrackerWebsite.Utility;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public partial class Startup
    {
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            // Request tracking middleware should be added as the very first middleware in pipeline
            app.UseApplicationInsightsRequestTelemetry();

            if (env.IsDevelopment() || this.Environment.IsBuddy())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            // Exception middleware should be added after error page and any other error handling middleware.
            app.UseApplicationInsightsExceptionTelemetry();

            // We want to start measuring latency as soon as possible during a request.
            app.UseMiddleware<MeasureLatencyMiddleware>();

            // Require https on non-devbox
            if (!env.IsDevelopment())
            {
                app.UseMiddleware<RequireHttpsMiddleware>();
            }

            app.UseStaticFiles();

            this.ConfigureAuthentication(app, env);

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

            this.EnsureDatabaseCreated(app);

            // Warm up the game data parsing
            app.ApplicationServices.GetService<GameData>();
        }
    }
}
