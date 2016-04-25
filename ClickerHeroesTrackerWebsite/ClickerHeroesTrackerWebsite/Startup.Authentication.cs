// <copyright file="Startup.Authentication.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite
{
    using ClickerHeroesTrackerWebsite.Authentication;
    using Microsoft.AspNet.Builder;
    using Microsoft.AspNet.Hosting;

    /// <summary>
    /// Configure authentication
    /// </summary>
    public partial class Startup
    {
        private void ConfigureAuthentication(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseIdentity();

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

            var facebookAppId = Configuration["Authentication:Facebook:AppId"];
            var facebookAppSecret = Configuration["Authentication:Facebook:AppSecret"];
            if (!string.IsNullOrEmpty(facebookAppId) && !string.IsNullOrEmpty(facebookAppSecret))
            {
                app.UseFacebookAuthentication(options =>
                {
                    options.AppId = facebookAppId;
                    options.AppSecret = facebookAppSecret;
                });
            }

            var googleClientId = Configuration["Authentication:Google:ClientId"];
            var googleClientSecret = Configuration["Authentication:Google:ClientSecret"];
            if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
            {
                app.UseGoogleAuthentication(options =>
                {
                    options.ClientId = googleClientId;
                    options.ClientSecret = googleClientSecret;
                });
            }

            var twitterConsumerKey = Configuration["Authentication:Twitter:ConsumerKey"];
            var twitterConsumerSecret = Configuration["Authentication:Twitter:ConsumerSecret"];
            if (!string.IsNullOrEmpty(twitterConsumerKey) && !string.IsNullOrEmpty(twitterConsumerSecret))
            {
                app.UseTwitterAuthentication(options =>
                {
                    options.ConsumerKey = twitterConsumerKey;
                    options.ConsumerSecret = twitterConsumerSecret;
                });
            }

            // Allow auth mocking when not in prod
            if (!env.IsProduction())
            {
                app.UseMiddleware<MockAuthenticationOwinMiddleware>();
            }
        }
    }
}