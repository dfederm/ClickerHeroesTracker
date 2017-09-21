// <copyright file="FacebookAssertionGrantHandlerTests.cs" company="Clicker Heroes Tracker">
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

    public sealed class FacebookAssertionGrantHandlerTests
    {
        private const string AppId = "SomeAppId";
        private const string ExternalUserId = "SomeExternalUserId";
        private const string Assertion = "SomeAssertion";
        private const string AppEndpoint = "https://graph.facebook.com/app/?access_token=" + Assertion;
        private const string UserEndpoint = "https://graph.facebook.com/me?fields=id&access_token=" + Assertion;

        [Fact]
        public async Task ValidateAsync_Success()
        {
            var authenticationSettings = new AuthenticationSettings
            {
                Facebook = new FacebookAuthenticationSettings
                {
                    AppId = AppId,
                },
            };
            var options = Options.Create(authenticationSettings);

            var httpClient = new MockHttpClient();
            httpClient.AddMockResponse(AppEndpoint, new FacebookApp { Id = AppId });
            httpClient.AddMockResponse(UserEndpoint, new FacebookUser { Id = ExternalUserId });

            var handler = new FacebookAssertionGrantHandler(options, httpClient);
            var result = await handler.ValidateAsync(Assertion);

            Assert.NotNull(result);
            Assert.True(result.IsSuccessful);
            Assert.Equal(ExternalUserId, result.ExternalUserId);

            httpClient.VerifyNoOutstandingRequests();
        }

        [Fact]
        public async Task ValidateAsync_HttpError_AppEndpoint()
        {
            var authenticationSettings = new AuthenticationSettings();
            var options = Options.Create(authenticationSettings);

            var httpClient = new MockHttpClient();
            httpClient.AddMockResponse(AppEndpoint, HttpStatusCode.BadRequest);

            var handler = new FacebookAssertionGrantHandler(options, httpClient);
            var result = await handler.ValidateAsync(Assertion);

            Assert.NotNull(result);
            Assert.False(result.IsSuccessful);

            httpClient.VerifyNoOutstandingRequests();
        }

        [Fact]
        public async Task ValidateAsync_HttpError_UserEndpoint()
        {
            var authenticationSettings = new AuthenticationSettings
            {
                Facebook = new FacebookAuthenticationSettings
                {
                    AppId = AppId,
                },
            };
            var options = Options.Create(authenticationSettings);

            var httpClient = new MockHttpClient();
            httpClient.AddMockResponse(AppEndpoint, new FacebookApp { Id = AppId });
            httpClient.AddMockResponse(UserEndpoint, HttpStatusCode.BadRequest);

            var handler = new FacebookAssertionGrantHandler(options, httpClient);
            var result = await handler.ValidateAsync(Assertion);

            Assert.NotNull(result);
            Assert.False(result.IsSuccessful);

            httpClient.VerifyNoOutstandingRequests();
        }

        [Fact]
        public async Task ValidateAsync_WrongApp()
        {
            var authenticationSettings = new AuthenticationSettings
            {
                Facebook = new FacebookAuthenticationSettings
                {
                    AppId = AppId,
                },
            };
            var options = Options.Create(authenticationSettings);

            var httpClient = new MockHttpClient();
            httpClient.AddMockResponse(AppEndpoint, new FacebookApp { Id = "SomeOtherAppId" });

            var handler = new FacebookAssertionGrantHandler(options, httpClient);
            var result = await handler.ValidateAsync(Assertion);

            Assert.NotNull(result);
            Assert.False(result.IsSuccessful);

            httpClient.VerifyNoOutstandingRequests();
        }
    }
}
