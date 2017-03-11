// <copyright file="Startup.Authentication.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite
{
    using ClickerHeroesTrackerWebsite.Services.Authentication;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Configure authentication
    /// </summary>
    public partial class Startup
    {
        private void ConfigureAuthentication(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseIdentity();
            app.UseOAuthValidation();

            var authenticationSettingsOptions = app.ApplicationServices.GetService<IOptions<AuthenticationSettings>>();
            if (authenticationSettingsOptions.Value != null)
            {
                var microsoftAuthenticationSettings = authenticationSettingsOptions.Value.Microsoft;
                if (microsoftAuthenticationSettings != null
                    && !string.IsNullOrEmpty(microsoftAuthenticationSettings.ClientId)
                    && !string.IsNullOrEmpty(microsoftAuthenticationSettings.ClientSecret))
                {
                    app.UseMicrosoftAccountAuthentication(new MicrosoftAccountOptions
                    {
                        ClientId = microsoftAuthenticationSettings.ClientId,
                        ClientSecret = microsoftAuthenticationSettings.ClientSecret,
                    });
                }

                var facebookAuthenticationSettings = authenticationSettingsOptions.Value.Facebook;
                if (facebookAuthenticationSettings != null
                    && !string.IsNullOrEmpty(facebookAuthenticationSettings.AppId)
                    && !string.IsNullOrEmpty(facebookAuthenticationSettings.AppSecret))
                {
                    app.UseFacebookAuthentication(new FacebookOptions
                    {
                        AppId = facebookAuthenticationSettings.AppId,
                        AppSecret = facebookAuthenticationSettings.AppSecret,
                    });
                }

                var googleAuthenticationSettings = authenticationSettingsOptions.Value.Google;
                if (googleAuthenticationSettings != null
                    && !string.IsNullOrEmpty(googleAuthenticationSettings.ClientId)
                    && !string.IsNullOrEmpty(googleAuthenticationSettings.ClientSecret))
                {
                    app.UseGoogleAuthentication(new GoogleOptions
                    {
                        ClientId = googleAuthenticationSettings.ClientId,
                        ClientSecret = googleAuthenticationSettings.ClientSecret,
                    });
                }

                var twitterAuthenticationSettings = authenticationSettingsOptions.Value.Twitter;
                if (twitterAuthenticationSettings != null
                    && !string.IsNullOrEmpty(twitterAuthenticationSettings.ConsumerKey)
                    && !string.IsNullOrEmpty(twitterAuthenticationSettings.ConsumerSecret))
                {
                    app.UseTwitterAuthentication(new TwitterOptions
                    {
                        ConsumerKey = twitterAuthenticationSettings.ConsumerKey,
                        ConsumerSecret = twitterAuthenticationSettings.ConsumerSecret,
                    });
                }
            }

            // Note: UseOpenIddict() must be registered after app.UseIdentity() and the external social providers.
            app.UseOpenIddict();

            // Allow auth mocking when not in prod
            if (!env.IsProduction())
            {
                app.UseMiddleware<MockAuthenticationOwinMiddleware>();
            }
        }
    }
}