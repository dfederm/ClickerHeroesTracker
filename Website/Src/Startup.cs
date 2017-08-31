// <copyright file="Startup.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite
{
    using ClickerHeroesTrackerWebsite.Instrumentation;
    using ClickerHeroesTrackerWebsite.Models.Game;
    using ClickerHeroesTrackerWebsite.Models.Settings;
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

            app.UseStaticFiles();

            app.UseAuthentication();

            // Instrument the user as soon as they're auth'd.
            app.UseMiddleware<UserInstrumentationMiddleware>();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}");

                // Frontend routes. Eventually just match a wildcard and let Angular render the 404
                routes.MapRoute(
                    name: "frontend-home",
                    template: "beta/{*path}",
                    defaults: new { controller = "Home", action = "Beta" });
            });

            app.UseSwagger(c =>
            {
                c.PreSerializeFilters.Add((swagger, httpReq) => swagger.Host = httpReq.Host.Value);
            });

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "V1 Docs");
            });

            this.EnsureDatabaseCreated(app);

            // Warm up the game data parsing
            app.ApplicationServices.GetService<GameData>();
        }
    }
}
