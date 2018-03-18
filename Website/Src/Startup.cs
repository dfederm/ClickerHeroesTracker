// <copyright file="Startup.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite
{
    using System;
    using System.Threading.Tasks;
    using ClickerHeroesTrackerWebsite.Models.Game;
    using ClickerHeroesTrackerWebsite.Utility;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Website.Services.SiteNews;

    public partial class Startup
    {
        private readonly IHostingEnvironment environment;
        private readonly IConfiguration configuration;

        public Startup(IHostingEnvironment environment, IConfiguration configuration)
        {
            this.environment = environment;
            this.configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            if (this.environment.IsDevelopment() || this.environment.IsBuddy())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            // Hint to brower to use https
            app.UseHsts();

            // Force server side to use https
            app.UseHttpsRedirection();

            var staticFileOptions = this.environment.IsDevelopment()
                ? new StaticFileOptions
                {
                    OnPrepareResponse = context =>
                    {
                        // Disable caching to help with development
                        context.Context.Response.Headers["Cache-Control"] = "no-cache, no-store";
                        context.Context.Response.Headers["Pragma"] = "no-cache";
                        context.Context.Response.Headers["Expires"] = "-1";
                    },
                }
                : new StaticFileOptions
                {
                    OnPrepareResponse = context =>
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

            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var serviceProvider = serviceScope.ServiceProvider;
                Task.WaitAll(
                    this.EnsureDatabaseCreatedAsync(serviceProvider),
                    serviceProvider.GetService<ISiteNewsProvider>().EnsureCreatedAsync(),
                    Task.Run(async () =>
                    {
                        // Break away from the parent content to ensure this happens in parallel.
                        await Task.Yield();

                        // Warm up the game data parsing
                        app.ApplicationServices.GetService<GameData>();
                    }));
            }
        }
    }
}
