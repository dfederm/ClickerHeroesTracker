// <copyright file="GoogleAssertionGrantHandler.cs" company="Clicker Heroes Tracker">
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

    public sealed class GoogleAssertionGrantHandler : IAssertionGrantHandler
    {
        private readonly AuthenticationSettings authenticationSettings;

        private readonly HttpClient httpClient;

        public GoogleAssertionGrantHandler(
            IOptions<AuthenticationSettings> authenticationSettings,
            HttpClient httpClient)
        {
            this.authenticationSettings = authenticationSettings.Value;
            this.httpClient = httpClient;
        }

        public static string GrantType => "urn:ietf:params:oauth:grant-type:google_identity_token";

        public string Name => "Google";

        public async Task<AssertionGrantResult> ValidateAsync(string assertion)
        {
            // Google's endpoint will validate the iss field, the exp field and the signature.
            JsonWebToken googleToken;
            var validationEndpoint = "https://www.googleapis.com/oauth2/v3/tokeninfo?id_token=" + assertion;
            using (var response = await this.httpClient.GetAsync(validationEndpoint))
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    return new AssertionGrantResult { Error = "Token validation failed" };
                }

                var stream = await response.Content.ReadAsStreamAsync();
                using (var reader = new JsonTextReader(new StreamReader(stream)))
                {
                    var serializer = new JsonSerializer();
                    googleToken = serializer.Deserialize<JsonWebToken>(reader);
                }
            }

            // We still validate the aud to ensure it's not a token for some other app
            if (!this.authenticationSettings.Google.ClientId.Equals(googleToken.Aud, StringComparison.OrdinalIgnoreCase))
            {
                return new AssertionGrantResult { Error = "The token was for the wrong audience" };
            }

            return new AssertionGrantResult { ExternalUserId = googleToken.Sub };
        }
    }
}
