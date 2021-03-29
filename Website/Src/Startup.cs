// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System;
using System.Threading.Tasks;
using ClickerHeroesTrackerWebsite.Models.Game;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Website.Services.SiteNews;

namespace ClickerHeroesTrackerWebsite
{
    public partial class Startup
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;

        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            _environment = environment;
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            if (_environment.IsDevelopment())
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

            StaticFileOptions staticFileOptions = _environment.IsDevelopment()
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

            // The point at which the routing decision is made.
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCors();

            // Executes the endpoint that was selected by routing.
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                // Fallback to the Angular app
                endpoints.MapFallbackToFile("/index.html");
            });

            using (IServiceScope serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                IServiceProvider serviceProvider = serviceScope.ServiceProvider;
                Task.WaitAll(
                    EnsureDatabaseCreatedAsync(serviceProvider),
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
