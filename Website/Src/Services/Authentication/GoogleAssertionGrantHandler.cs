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
    public sealed class GoogleAssertionGrantHandler : IAssertionGrantHandler
    {
        private readonly AuthenticationSettings _authenticationSettings;

        private readonly HttpClient _httpClient;

        public GoogleAssertionGrantHandler(
            IOptions<AuthenticationSettings> authenticationSettings,
            HttpClient httpClient)
        {
            _authenticationSettings = authenticationSettings.Value;
            _httpClient = httpClient;
        }

        public static string GrantType => "urn:ietf:params:oauth:grant-type:google_identity_token";

        public string Name => "Google";

        public async Task<AssertionGrantResult> ValidateAsync(string assertion)
        {
            // Google's endpoint will validate the iss field, the exp field and the signature.
            JsonWebToken googleToken;
            string validationEndpoint = "https://www.googleapis.com/oauth2/v3/tokeninfo?id_token=" + assertion;
            using (HttpResponseMessage response = await _httpClient.GetAsync(validationEndpoint))
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    return new AssertionGrantResult { Error = "Token validation failed" };
                }

                Stream stream = await response.Content.ReadAsStreamAsync();
                using (JsonTextReader reader = new(new StreamReader(stream)))
                {
                    JsonSerializer serializer = new();
                    googleToken = serializer.Deserialize<JsonWebToken>(reader);
                }
            }

            // We still validate the aud to ensure it's not a token for some other app
            if (!_authenticationSettings.Google.ClientId.Equals(googleToken.Aud, StringComparison.OrdinalIgnoreCase))
            {
                return new AssertionGrantResult { Error = "The token was for the wrong audience" };
            }

            return new AssertionGrantResult { ExternalUserId = googleToken.Sub, ExternalUserEmail = googleToken.Email };
        }
    }
}
