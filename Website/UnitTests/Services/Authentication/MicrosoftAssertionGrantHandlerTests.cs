// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Testing.HttpClient;
using Website.Models.Authentication;
using Website.Services.Authentication;
using Xunit;

namespace UnitTests.Services.Authentication
{
    public sealed class MicrosoftAssertionGrantHandlerTests
    {
        private const string ClientId = "SomeClientId";
        private const string ExternalUserId = "SomeExternalUserId";
        private const string ExternalUserEmail = "SomeExternalUserEmail";
        private const string ConfigurationEndpoint = "https://login.microsoftonline.com/common/v2.0/.well-known/openid-configuration";
        private const string KeysEndpoint = "https://login.microsoftonline.com/common/discovery/v2.0/keys";

        // Taken from http://self-issued.info/docs/draft-ietf-jose-json-web-key.html#rfc.appendix.A.2
        private static readonly JsonWebKey JsonWebKey = new()
        {
            Kty = "RSA",
            N = "0vx7agoebGcQSuuPiLJXZptN9nndrQmbXEps2aiAFbWhM78LhWx4cbbfAAtVT86zwu1RK7aPFFxuhDR1L6tSoc_BJECPebWKRXjBZCiFV4n3oknjhMstn64tZ_2W-5JsGY4Hc5n9yBXArwl93lqt7_RN5w6Cf0h4QyQ5v-65YGjQR0_FDW2QvzqY368QQMicAtaSqzs8KJZgnYb9c7d0zgdAZHzu6qMQvRL5hajrn1n91CbOpbISD08qNLyrdkt-bFTWhAI4vMQFh6WeZu0fM4lFd2NcRwr3XPksINHaQ-G_xBniIqbw0Ls1jF44-csFCur-kEgU8awapJzKnqDKgw",
            E = "AQAB",
            D = "X4cTteJY_gn4FYPsXB8rdXix5vwsg1FLN5E3EaG6RJoVH-HLLKD9M7dx5oo7GURknchnrRweUkC7hT5fJLM0WbFAKNLWY2vv7B6NqXSzUvxT0_YSfqijwp3RTzlBaCxWp4doFk5N2o8Gy_nHNKroADIkJ46pRUohsXywbReAdYaMwFs9tv8d_cPVY3i07a3t8MN6TNwm0dSawm9v47UiCl3Sk5ZiG7xojPLu4sbg1U2jx4IBTNBznbJSzFHK66jT8bgkuqsk0GjskDJk19Z4qwjwbsnn4j2WBii3RL-Us2lGVkY8fkFzme1z0HbIkfz0Y6mqnOYtqc0X4jfcKoAC8Q",
            P = "83i-7IvMGXoMXCskv73TKr8637FiO7Z27zv8oj6pbWUQyLPQBQxtPVnwD20R-60eTDmD2ujnMt5PoqMrm8RfmNhVWDtjjMmCMjOpSXicFHj7XOuVIYQyqVWlWEh6dN36GVZYk93N8Bc9vY41xy8B9RzzOGVQzXvNEvn7O0nVbfs",
            Q = "3dfOR9cuYq-0S-mkFLzgItgMEfFzB2q3hWehMuG0oCuqnb3vobLyumqjVZQO1dIrdwgTnCdpYzBcOfW5r370AFXjiWft_NGEiovonizhKpo9VVS78TzFgxkIdrecRezsZ-1kYd_s1qDbxtkDEgfAITAG9LUnADun4vIcb6yelxk",
            DP = "G4sPXkc6Ya9y8oJW9_ILj4xuppu0lzi_H7VTkS8xj5SdX3coE0oimYwxIi2emTAue0UOa5dpgFGyBJ4c8tQ2VF402XRugKDTP8akYhFo5tAA77Qe_NmtuYZc3C3m3I24G2GvR5sSDxUyAN2zq8Lfn9EUms6rY3Ob8YeiKkTiBj0",
            DQ = "s9lAH9fggBsoFR8Oac2R_E2gw282rT2kGOAhvIllETE1efrA6huUUvMfBcMpn8lqeW6vzznYY5SSQF7pMdC_agI3nG8Ibp1BUb0JUiraRNqUfLhcQb_d9GF4Dh7e74WbRsobRonujTYN1xCaP6TO61jvWrX-L18txXw494Q_cgk",
            QI = "GyM_p6JrXySiz1toFgKbWV-JdI3jQ4ypu9rbMWx3rQJBfmt0FoYzgUIZEVFEcOqwemRN81zoDAaa-Bk0KWNGDjJHZDdDmFhW3AN7lI-puxk_mHZGJ11rxyR8O55XLSe3SPmRfKwZI6yU24ZxvQKFYItdldUKGzO6Ia6zTKhAVRU",
            Alg = "RS256",
            Kid = "SomeKid",
        };

        [Fact]
        public async Task ValidateAsync_Success()
        {
            AuthenticationSettings authenticationSettings = new()
            {
                Microsoft = new MicrosoftAuthenticationSettings
                {
                    ClientId = ClientId,
                },
            };
            IOptions<AuthenticationSettings> options = Options.Create(authenticationSettings);

            OpenIdConnectConfiguration configuration = new();
            configuration.JwksUri = KeysEndpoint;

            JsonWebKeySet jsonWebKeySet = new();
            jsonWebKeySet.Keys.Add(JsonWebKey);

            using (HttpClientTestingFactory http = new())
            {
                JwtSecurityTokenHandler tokenHandler = new();
                JwtSecurityToken token = new(
                    audience: ClientId,
                    claims: new[] { new Claim("sub", ExternalUserId), new Claim("email", ExternalUserEmail) },
                    notBefore: DateTime.UtcNow,
                    expires: DateTime.UtcNow + TimeSpan.FromHours(1),
                    signingCredentials: new SigningCredentials(JsonWebKey, JsonWebKey.Alg));

                MicrosoftAssertionGrantHandler handler = new(options, http.HttpClient);
                Task<AssertionGrantResult> resultTask = handler.ValidateAsync(tokenHandler.WriteToken(token));

                await Task.Delay(100); // HACK! Allow time for the request to be sent
                http.Expect(ConfigurationEndpoint).Respond(OpenIdConnectConfiguration.Write(configuration));

                await Task.Delay(100); // HACK! Allow time for the request to be sent
                http.Expect(KeysEndpoint).Respond(JsonSerializer.Serialize(jsonWebKeySet));

                AssertionGrantResult result = await resultTask;
                Assert.NotNull(result);
                Assert.True(result.IsSuccessful);
                Assert.Equal(ExternalUserId, result.ExternalUserId);
                Assert.Equal(ExternalUserEmail, result.ExternalUserEmail);

                http.EnsureNoOutstandingRequests();
            }
        }

        [Fact]
        public async Task ValidateAsync_WrongAudience()
        {
            AuthenticationSettings authenticationSettings = new()
            {
                Microsoft = new MicrosoftAuthenticationSettings
                {
                    ClientId = ClientId,
                },
            };
            IOptions<AuthenticationSettings> options = Options.Create(authenticationSettings);

            OpenIdConnectConfiguration configuration = new();
            configuration.JwksUri = KeysEndpoint;

            JsonWebKeySet jsonWebKeySet = new();
            jsonWebKeySet.Keys.Add(JsonWebKey);

            using (HttpClientTestingFactory http = new())
            {
                JwtSecurityTokenHandler tokenHandler = new();
                JwtSecurityToken token = new(
                    audience: "SomeOtherClientId",
                    claims: new[] { new Claim("sub", ExternalUserId), new Claim("email", ExternalUserEmail) },
                    notBefore: DateTime.UtcNow,
                    expires: DateTime.UtcNow + TimeSpan.FromHours(1),
                    signingCredentials: new SigningCredentials(JsonWebKey, JsonWebKey.Alg));

                MicrosoftAssertionGrantHandler handler = new(options, http.HttpClient);
                Task<AssertionGrantResult> resultTask = handler.ValidateAsync(tokenHandler.WriteToken(token));

                await Task.Delay(100); // HACK! Allow time for the request to be sent
                http.Expect(ConfigurationEndpoint).Respond(OpenIdConnectConfiguration.Write(configuration));

                await Task.Delay(100); // HACK! Allow time for the request to be sent
                http.Expect(KeysEndpoint).Respond(JsonSerializer.Serialize(jsonWebKeySet));

                AssertionGrantResult result = await resultTask;
                Assert.NotNull(result);
                Assert.False(result.IsSuccessful);

                http.EnsureNoOutstandingRequests();
            }
        }

        [Fact]
        public async Task ValidateAsync_InvalidToken()
        {
            AuthenticationSettings authenticationSettings = new()
            {
                Microsoft = new MicrosoftAuthenticationSettings
                {
                    ClientId = ClientId,
                },
            };
            IOptions<AuthenticationSettings> options = Options.Create(authenticationSettings);

            OpenIdConnectConfiguration configuration = new();
            configuration.JwksUri = KeysEndpoint;

            JsonWebKeySet jsonWebKeySet = new();
            jsonWebKeySet.Keys.Add(JsonWebKey);

            using (HttpClientTestingFactory http = new())
            {
                MicrosoftAssertionGrantHandler handler = new(options, http.HttpClient);
                Task<AssertionGrantResult> resultTask = handler.ValidateAsync("SomeBadAssertion");

                await Task.Delay(100); // HACK! Allow time for the request to be sent
                http.Expect(ConfigurationEndpoint).Respond(OpenIdConnectConfiguration.Write(configuration));

                await Task.Delay(100); // HACK! Allow time for the request to be sent
                http.Expect(KeysEndpoint).Respond(JsonSerializer.Serialize(jsonWebKeySet));

                AssertionGrantResult result = await resultTask;
                Assert.NotNull(result);
                Assert.False(result.IsSuccessful);

                http.EnsureNoOutstandingRequests();
            }
        }
    }
}
