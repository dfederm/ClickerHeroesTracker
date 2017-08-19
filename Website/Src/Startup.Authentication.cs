// <copyright file="Startup.Authentication.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite
{
    using AspNet.Security.OAuth.Validation;
    using ClickerHeroesTrackerWebsite.Services.Authentication;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Configure authentication
    /// </summary>
    public partial class Startup
    {
        private void ConfigureAuthentication(IServiceCollection services)
        {
            var authenticationBuilder = services.AddAuthentication();

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

            var twitterConsumerKey = this.Configuration["Authentication:Twitter:ConsumerKey"];
            var twitterConsumerSecret = this.Configuration["Authentication:Twitter:ConsumerSecret"];
            if (!string.IsNullOrEmpty(twitterConsumerKey) && !string.IsNullOrEmpty(twitterConsumerSecret))
            {
                authenticationBuilder.AddTwitter(options =>
                {
                    options.ConsumerKey = twitterConsumerKey;
                    options.ConsumerSecret = twitterConsumerSecret;
                });
            }

            authenticationBuilder.AddScheme<AuthenticationSchemeOptions, MockAuthenticationHandler>("Mock", options => { });

            services.AddAuthorization(options =>
            {
                var policyBuilder = new AuthorizationPolicyBuilder();
                policyBuilder.RequireAuthenticatedUser();

                // Allow both the application cookies and bearer tokens for auth
                policyBuilder.AddAuthenticationSchemes(IdentityConstants.ApplicationScheme, OAuthValidationDefaults.AuthenticationScheme);

                // In non-production, allow mock auth as well
                if (!this.Environment.IsProduction())
                {
                    policyBuilder.AddAuthenticationSchemes("Mock");
                }

                options.DefaultPolicy = policyBuilder.Build();
            });
        }
    }
}