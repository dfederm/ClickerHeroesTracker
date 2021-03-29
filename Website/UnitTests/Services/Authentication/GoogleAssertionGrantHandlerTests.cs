// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Testing.HttpClient;
using Website.Models.Authentication;
using Website.Services.Authentication;
using Xunit;

namespace UnitTests.Services.Authentication
{
    public static class GoogleAssertionGrantHandlerTests
    {
        private const string ClientId = "SomeClientId";
        private const string ExternalUserId = "SomeExternalUserId";
        private const string ExternalUserEmail = "SomeExternalUserEmail";
        private const string Assertion = "SomeAssertion";
        private const string ValidationEndpoint = "https://www.googleapis.com/oauth2/v3/tokeninfo?id_token=" + Assertion;

        [Fact]
        public static async Task ValidateAsync_Success()
        {
            AuthenticationSettings authenticationSettings = new()
            {
                Google = new GoogleAuthenticationSettings
                {
                    ClientId = ClientId,
                },
            };
            IOptions<AuthenticationSettings> options = Options.Create(authenticationSettings);

            using (HttpClientTestingFactory http = new())
            {
                GoogleAssertionGrantHandler handler = new(options, http.HttpClient);
                Task<AssertionGrantResult> resultTask = handler.ValidateAsync(Assertion);

                http.Expect(ValidationEndpoint).Respond(JsonConvert.SerializeObject(new JsonWebToken
                {
                    Aud = ClientId,
                    Sub = ExternalUserId,
                    Email = ExternalUserEmail,
                }));

                AssertionGrantResult result = await resultTask;
                Assert.NotNull(result);
                Assert.True(result.IsSuccessful);
                Assert.Equal(ExternalUserId, result.ExternalUserId);
                Assert.Equal(ExternalUserEmail, result.ExternalUserEmail);

                http.EnsureNoOutstandingRequests();
            }
        }

        [Fact]
        public static async Task ValidateAsync_HttpError()
        {
            AuthenticationSettings authenticationSettings = new();
            IOptions<AuthenticationSettings> options = Options.Create(authenticationSettings);

            using (HttpClientTestingFactory http = new())
            {
                GoogleAssertionGrantHandler handler = new(options, http.HttpClient);
                Task<AssertionGrantResult> resultTask = handler.ValidateAsync(Assertion);

                http.Expect(ValidationEndpoint).Respond(HttpStatusCode.BadRequest);

                AssertionGrantResult result = await resultTask;
                Assert.NotNull(result);
                Assert.False(result.IsSuccessful);

                http.EnsureNoOutstandingRequests();
            }
        }

        [Fact]
        public static async Task ValidateAsync_WrongAudience()
        {
            AuthenticationSettings authenticationSettings = new()
            {
                Google = new GoogleAuthenticationSettings
                {
                    ClientId = ClientId,
                },
            };
            IOptions<AuthenticationSettings> options = Options.Create(authenticationSettings);

            using (HttpClientTestingFactory http = new())
            {
                GoogleAssertionGrantHandler handler = new(options, http.HttpClient);
                Task<AssertionGrantResult> resultTask = handler.ValidateAsync(Assertion);

                http.Expect(ValidationEndpoint).Respond(JsonConvert.SerializeObject(new JsonWebToken
                {
                    Aud = "SomeOtherClientId",
                    Sub = ExternalUserId,
                }));

                AssertionGrantResult result = await resultTask;
                Assert.NotNull(result);
                Assert.False(result.IsSuccessful);

                http.EnsureNoOutstandingRequests();
            }
        }
    }
}
