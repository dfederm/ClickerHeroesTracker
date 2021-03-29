// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Website.Models.Authentication;

namespace Website.Services.Authentication
{
    public sealed class FacebookAssertionGrantHandler : IAssertionGrantHandler
    {
        private readonly AuthenticationSettings _authenticationSettings;

        private readonly HttpClient _httpClient;

        public FacebookAssertionGrantHandler(
            IOptions<AuthenticationSettings> authenticationSettings,
            HttpClient httpClient)
        {
            _authenticationSettings = authenticationSettings.Value;
            _httpClient = httpClient;
        }

        public static string GrantType => "urn:ietf:params:oauth:grant-type:facebook_access_token";

        public string Name => "Facebook";

        public async Task<AssertionGrantResult> ValidateAsync(string assertion)
        {
            // Verify the token is for our app. This also indirectly verifies the token is valid since it's useable.
            string appEndpoint = "https://graph.facebook.com/app/?access_token=" + assertion;
            using (HttpResponseMessage response = await _httpClient.GetAsync(appEndpoint))
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    return new AssertionGrantResult { Error = "Token validation failed" };
                }

                Stream stream = await response.Content.ReadAsStreamAsync();
                using (JsonTextReader reader = new(new StreamReader(stream)))
                {
                    JsonSerializer serializer = new();
                    FacebookApp facebookApp = serializer.Deserialize<FacebookApp>(reader);
                    if (!_authenticationSettings.Facebook.AppId.Equals(facebookApp.Id, StringComparison.OrdinalIgnoreCase))
                    {
                        return new AssertionGrantResult { Error = "The token was for the wrong app" };
                    }
                }
            }

            // Get the facebook user id. We also have it on the client already but we can't trust clients to tell us who they are.
            FacebookUser facebookUser;
            string userEndpoint = "https://graph.facebook.com/me?fields=id,email&access_token=" + assertion;
            using (HttpResponseMessage response = await _httpClient.GetAsync(userEndpoint))
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    return new AssertionGrantResult { Error = "Token validation failed" };
                }

                Stream stream = await response.Content.ReadAsStreamAsync();
                using (JsonTextReader reader = new(new StreamReader(stream)))
                {
                    JsonSerializer serializer = new();
                    facebookUser = serializer.Deserialize<FacebookUser>(reader);
                }
            }

            if (string.IsNullOrEmpty(facebookUser.Id))
            {
                return new AssertionGrantResult { Error = "The token does not belong to a valid user" };
            }

            return new AssertionGrantResult { ExternalUserId = facebookUser.Id, ExternalUserEmail = facebookUser.Email };
        }
    }
}
