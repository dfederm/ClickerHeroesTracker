// <copyright file="GoogleAssertionGrantHandlerTests.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace UnitTests.Services.Authentication
{
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Options;
    using UnitTests.Mocks;
    using Website.Models.Authentication;
    using Website.Services.Authentication;
    using Xunit;

    public sealed class GoogleAssertionGrantHandlerTests
    {
        private const string ClientId = "SomeClientId";
        private const string ExternalUserId = "SomeExternalUserId";
        private const string ExternalUserEmail = "SomeExternalUserEmail";
        private const string Assertion = "SomeAssertion";
        private const string ValidationEndpoint = "https://www.googleapis.com/oauth2/v3/tokeninfo?id_token=" + Assertion;

        [Fact]
        public async Task ValidateAsync_Success()
        {
            var authenticationSettings = new AuthenticationSettings
            {
                Google = new GoogleAuthenticationSettings
                {
                    ClientId = ClientId,
                },
            };
            var options = Options.Create(authenticationSettings);

            var httpClient = new MockHttpClient();
            httpClient.AddMockResponse(ValidationEndpoint, new JsonWebToken
            {
                Aud = ClientId,
                Sub = ExternalUserId,
                Email = ExternalUserEmail,
            });

            var handler = new GoogleAssertionGrantHandler(options, httpClient);
            var result = await handler.ValidateAsync(Assertion);

            Assert.NotNull(result);
            Assert.True(result.IsSuccessful);
            Assert.Equal(ExternalUserId, result.ExternalUserId);
            Assert.Equal(ExternalUserEmail, result.ExternalUserEmail);

            httpClient.VerifyNoOutstandingRequests();
        }

        [Fact]
        public async Task ValidateAsync_HttpError()
        {
            var authenticationSettings = new AuthenticationSettings();
            var options = Options.Create(authenticationSettings);

            var httpClient = new MockHttpClient();
            httpClient.AddMockResponse(ValidationEndpoint, HttpStatusCode.BadRequest);

            var handler = new GoogleAssertionGrantHandler(options, httpClient);
            var result = await handler.ValidateAsync(Assertion);

            Assert.NotNull(result);
            Assert.False(result.IsSuccessful);

            httpClient.VerifyNoOutstandingRequests();
        }

        [Fact]
        public async Task ValidateAsync_WrongAudience()
        {
            var authenticationSettings = new AuthenticationSettings
            {
                Google = new GoogleAuthenticationSettings
                {
                    ClientId = ClientId,
                },
            };
            var options = Options.Create(authenticationSettings);

            var httpClient = new MockHttpClient();
            httpClient.AddMockResponse(ValidationEndpoint, new JsonWebToken
            {
                Aud = "SomeOtherClientId",
                Sub = ExternalUserId,
            });

            var handler = new GoogleAssertionGrantHandler(options, httpClient);
            var result = await handler.ValidateAsync(Assertion);

            Assert.NotNull(result);
            Assert.False(result.IsSuccessful);

            httpClient.VerifyNoOutstandingRequests();
        }
    }
}
