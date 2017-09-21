// <copyright file="FacebookAssertionGrantHandler.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace Website.Services.Authentication
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Website.Models.Authentication;

    public sealed class FacebookAssertionGrantHandler : IAssertionGrantHandler
    {
        private readonly AuthenticationSettings authenticationSettings;

        private readonly HttpClient httpClient;

        public FacebookAssertionGrantHandler(
            IOptions<AuthenticationSettings> authenticationSettings,
            HttpClient httpClient)
        {
            this.authenticationSettings = authenticationSettings.Value;
            this.httpClient = httpClient;
        }

        public static string GrantType => "urn:ietf:params:oauth:grant-type:facebook_access_token";

        public string Name => "Facebook";

        public async Task<AssertionGrantResult> ValidateAsync(string assertion)
        {
            // Verify the token is for our app. This also indirectly verifies the token is valid since it's useable.
            var appEndpoint = "https://graph.facebook.com/app/?access_token=" + assertion;
            using (var response = await this.httpClient.GetAsync(appEndpoint))
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    return new AssertionGrantResult { Error = "Token validation failed" };
                }

                var stream = await response.Content.ReadAsStreamAsync();
                using (var reader = new JsonTextReader(new StreamReader(stream)))
                {
                    var serializer = new JsonSerializer();
                    var facebookApp = serializer.Deserialize<FacebookApp>(reader);
                    if (!this.authenticationSettings.Facebook.AppId.Equals(facebookApp.Id, StringComparison.OrdinalIgnoreCase))
                    {
                        return new AssertionGrantResult { Error = "The token was for the wrong app" };
                    }
                }
            }

            // Get the facebook user id. We also have it on the client already but we can't trust clients to tell us who they are.
            FacebookUser facebookUser;
            var userEndpoint = "https://graph.facebook.com/me?fields=id&access_token=" + assertion;
            using (var response = await this.httpClient.GetAsync(userEndpoint))
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    return new AssertionGrantResult { Error = "Token validation failed" };
                }

                var stream = await response.Content.ReadAsStreamAsync();
                using (var reader = new JsonTextReader(new StreamReader(stream)))
                {
                    var serializer = new JsonSerializer();
                    facebookUser = serializer.Deserialize<FacebookUser>(reader);
                }
            }

            if (string.IsNullOrEmpty(facebookUser.Id))
            {
                return new AssertionGrantResult { Error = "The token does not belong to a valid user" };
            }

            return new AssertionGrantResult { ExternalUserId = facebookUser.Id };
        }
    }
}
