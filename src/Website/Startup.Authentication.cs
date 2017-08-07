// <copyright file="Startup.Authentication.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite
{
    using ClickerHeroesTrackerWebsite.Services.Authentication;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Configure authentication
    /// </summary>
    public partial class Startup
    {
        private void ConfigureAuthentication(IServiceCollection services)
        {
            var authenticationBuilder = services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme);

            authenticationBuilder.AddCookie(options => options.LoginPath = new PathString("/Account/Login"));
            authenticationBuilder.AddOAuthValidation();

            var microsoftClientId = this.Configuration["Authentication:Microsoft:ClientId"];
            var microsoftClientSecret = this.Configuration["Authentication:Microsoft:ClientSecret"];
            if (!string.IsNullOrEmpty(microsoftClientId) && !string.IsNullOrEmpty(microsoftClientSecret))
            {
                authenticationBuilder.AddMicrosoftAccount(options =>
                {
                    options.ClientId = microsoftClientId;
                    options.ClientSecret = microsoftClientSecret;
                });
            }

            var facebookAppId = this.Configuration["Authentication:Facebook:AppId"];
            var facebookAppSecret = this.Configuration["Authentication:Facebook:AppSecret"];
            if (!string.IsNullOrEmpty(facebookAppId) && !string.IsNullOrEmpty(facebookAppSecret))
            {
                authenticationBuilder.AddFacebook(options =>
                {
                    options.AppId = facebookAppId;
                    options.AppSecret = facebookAppSecret;
                });
            }

            var googleClientId = this.Configuration["Authentication:Google:ClientId"];
            var googleClientSecret = this.Configuration["Authentication:Google:ClientSecret"];
            if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
            {
                authenticationBuilder.AddGoogle(options =>
                {
                    options.ClientId = googleClientId;
                    options.ClientSecret = googleClientSecret;
                });
            }

            var twitterConsumerKey = this.Configuration["Authentication:Facebook:ConsumerKey"];
            var twitterConsumerSecret = this.Configuration["Authentication:Facebook:ConsumerSecret"];
            if (!string.IsNullOrEmpty(twitterConsumerKey) && !string.IsNullOrEmpty(twitterConsumerSecret))
            {
                authenticationBuilder.AddTwitter(options =>
                {
                    options.ConsumerKey = twitterConsumerKey;
                    options.ConsumerSecret = twitterConsumerSecret;
                });
            }

            // Allow auth mocking when not in prod
            if (!this.Environment.IsProduction())
            {
                authenticationBuilder.AddScheme<AuthenticationSchemeOptions, MockAuthenticationHandler>("Mock", options =>
                {
                    options.ClaimsIssuer = "Mock";
                });
            }
        }
    }
}