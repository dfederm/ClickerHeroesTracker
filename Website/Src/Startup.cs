// <copyright file="Startup.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite
{
    using System;
    using ClickerHeroesTrackerWebsite.Instrumentation;
    using ClickerHeroesTrackerWebsite.Models.Game;
    using ClickerHeroesTrackerWebsite.Utility;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Rewrite;
    using Microsoft.Extensions.DependencyInjection;

    public partial class Startup
    {
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            if (this.Environment.IsDevelopment() || this.Environment.IsBuddy())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            // We want to start measuring latency as soon as possible during a request.
            app.UseMiddleware<MeasureLatencyMiddleware>();

            // Require https on non-devbox
            if (!this.Environment.IsDevelopment())
            {
                var options = new RewriteOptions()
                    .AddRedirectToHttpsPermanent();
                app.UseRewriter(options);
            }

            var staticFileOptions = this.Environment.IsDevelopment()
                ? new StaticFileOptions()
                {
                    OnPrepareResponse = (context) =>
                    {
                        // Disable caching to help with development
                        context.Context.Response.Headers["Cache-Control"] = "no-cache, no-store";
                        context.Context.Response.Headers["Pragma"] = "no-cache";
                        context.Context.Response.Headers["Expires"] = "-1";
                    },
                }
                : new StaticFileOptions()
                {
                    OnPrepareResponse = (context) =>
                    {
                        // Cache for a year
                        context.Context.Response.Headers["Cache-Control"] = "public,max-age=31536000";
                        context.Context.Response.Headers["Expires"] = DateTime.UtcNow.AddYears(1).ToString("R");
                    },
                };
            app.UseStaticFiles(staticFileOptions);

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                // All other controllers use attribute routing, so fallback everything else to the Angular app
                routes.MapRoute(
                    name: "frontend",
                    template: "{*path}",
                    defaults: new { controller = "Frontend", action = "Index" });
            });

            this.EnsureDatabaseCreated(app);

            // Warm up the game data parsing
            app.ApplicationServices.GetService<GameData>();
        }
    }
}
