// <copyright file="GoogleAssertionGrantHandlerTests.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace UnitTests.Services.Authentication
{
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Testing.HttpClient;
    using Website.Models.Authentication;
    using Website.Services.Authentication;
    using Xunit;

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
            var authenticationSettings = new AuthenticationSettings
            {
                Google = new GoogleAuthenticationSettings
                {
                    ClientId = ClientId,
                },
            };
            var options = Options.Create(authenticationSettings);

            using (var http = new HttpClientTestingFactory())
            {
                var handler = new GoogleAssertionGrantHandler(options, http.HttpClient);
                var resultTask = handler.ValidateAsync(Assertion);

                http.Expect(ValidationEndpoint).Respond(JsonConvert.SerializeObject(new JsonWebToken
                {
                    Aud = ClientId,
                    Sub = ExternalUserId,
                    Email = ExternalUserEmail,
                }));

                var result = await resultTask;
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
            var authenticationSettings = new AuthenticationSettings();
            var options = Options.Create(authenticationSettings);

            using (var http = new HttpClientTestingFactory())
            {
                var handler = new GoogleAssertionGrantHandler(options, http.HttpClient);
                var resultTask = handler.ValidateAsync(Assertion);

                http.Expect(ValidationEndpoint).Respond(HttpStatusCode.BadRequest);

                var result = await resultTask;
                Assert.NotNull(result);
                Assert.False(result.IsSuccessful);

                http.EnsureNoOutstandingRequests();
            }
        }

        [Fact]
        public static async Task ValidateAsync_WrongAudience()
        {
            var authenticationSettings = new AuthenticationSettings
            {
                Google = new GoogleAuthenticationSettings
                {
                    ClientId = ClientId,
                },
            };
            var options = Options.Create(authenticationSettings);

            using (var http = new HttpClientTestingFactory())
            {
                var handler = new GoogleAssertionGrantHandler(options, http.HttpClient);
                var resultTask = handler.ValidateAsync(Assertion);

                http.Expect(ValidationEndpoint).Respond(JsonConvert.SerializeObject(new JsonWebToken
                {
                    Aud = "SomeOtherClientId",
                    Sub = ExternalUserId,
                }));

                var result = await resultTask;
                Assert.NotNull(result);
                Assert.False(result.IsSuccessful);

                http.EnsureNoOutstandingRequests();
            }
        }
    }
}
