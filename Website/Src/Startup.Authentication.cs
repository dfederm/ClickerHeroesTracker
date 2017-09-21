// <copyright file="Startup.Authentication.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite
{
    using AspNet.Security.OAuth.Validation;
    using ClickerHeroesTrackerWebsite.Services.Authentication;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Website.Models.Authentication;

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

            // We can't access this via DI since we're still setting up the container, so manually create and bind the settings object.
            var authenticationSettings = new AuthenticationSettings();
            this.Configuration.GetSection("Authentication").Bind(authenticationSettings);

            var microsoftAuthenticationSettings = authenticationSettings.Microsoft;
            if (microsoftAuthenticationSettings != null
                && !string.IsNullOrEmpty(microsoftAuthenticationSettings.ClientId)
                && !string.IsNullOrEmpty(microsoftAuthenticationSettings.ClientSecret))
            {
                authenticationBuilder.AddMicrosoftAccount(options =>
                {
                    options.ClientId = microsoftAuthenticationSettings.ClientId;
                    options.ClientSecret = microsoftAuthenticationSettings.ClientSecret;
                });
            }

            var facebookAuthenticationSettings = authenticationSettings.Facebook;
            if (facebookAuthenticationSettings != null
                && !string.IsNullOrEmpty(facebookAuthenticationSettings.AppId)
                && !string.IsNullOrEmpty(facebookAuthenticationSettings.AppSecret))
            {
                authenticationBuilder.AddFacebook(options =>
                {
                    options.AppId = facebookAuthenticationSettings.AppId;
                    options.AppSecret = facebookAuthenticationSettings.AppSecret;
                });
            }

            var googleAuthenticationSettings = authenticationSettings.Google;
            if (googleAuthenticationSettings != null
                && !string.IsNullOrEmpty(googleAuthenticationSettings.ClientId)
                && !string.IsNullOrEmpty(googleAuthenticationSettings.ClientSecret))
            {
                authenticationBuilder.AddGoogle(options =>
                {
                    options.ClientId = googleAuthenticationSettings.ClientId;
                    options.ClientSecret = googleAuthenticationSettings.ClientSecret;
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