// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System;
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
    public static class FacebookAssertionGrantHandlerTests
    {
        private const string AppId = "SomeAppId";
        private const string ExternalUserId = "SomeExternalUserId";
        private const string ExternalUserEmail = "SomeExternalUserEmail";
        private const string Assertion = "SomeAssertion";
        private const string AppEndpoint = "https://graph.facebook.com/app/?access_token=" + Assertion;
        private const string UserEndpoint = "https://graph.facebook.com/me?fields=id,email&access_token=" + Assertion;

        // Upping the match timeout since this makes sequential http calls which for some reason may take more than the default of 100ms to chain together.
        private static readonly HttpClientTestingFactorySettings HttpClientTestingFactorySettings = new() { ExpectationMatchTimeout = TimeSpan.FromSeconds(10) };

        [Fact]
        public static async Task ValidateAsync_Success()
        {
            AuthenticationSettings authenticationSettings = new()
            {
                Facebook = new FacebookAuthenticationSettings
                {
                    AppId = AppId,
                },
            };
            IOptions<AuthenticationSettings> options = Options.Create(authenticationSettings);

            using (HttpClientTestingFactory http = new(HttpClientTestingFactorySettings))
            {
                FacebookAssertionGrantHandler handler = new(options, http.HttpClient);
                Task<AssertionGrantResult> resultTask = handler.ValidateAsync(Assertion);

                http.Expect(AppEndpoint).Respond(JsonConvert.SerializeObject(new FacebookApp { Id = AppId }));
                http.Expect(UserEndpoint).Respond(JsonConvert.SerializeObject(new FacebookUser { Id = ExternalUserId, Email = ExternalUserEmail }));

                AssertionGrantResult result = await resultTask;
                Assert.NotNull(result);
                Assert.True(result.IsSuccessful);
                Assert.Equal(ExternalUserId, result.ExternalUserId);
                Assert.Equal(ExternalUserEmail, result.ExternalUserEmail);

                http.EnsureNoOutstandingRequests();
            }
        }

        [Fact]
        public static async Task ValidateAsync_HttpError_AppEndpoint()
        {
            AuthenticationSettings authenticationSettings = new();
            IOptions<AuthenticationSettings> options = Options.Create(authenticationSettings);

            using (HttpClientTestingFactory http = new(HttpClientTestingFactorySettings))
            {
                FacebookAssertionGrantHandler handler = new(options, http.HttpClient);
                Task<AssertionGrantResult> resultTask = handler.ValidateAsync(Assertion);

                http.Expect(AppEndpoint).Respond(HttpStatusCode.BadRequest);

                AssertionGrantResult result = await resultTask;
                Assert.NotNull(result);
                Assert.False(result.IsSuccessful);

                http.EnsureNoOutstandingRequests();
            }
        }

        [Fact]
        public static async Task ValidateAsync_HttpError_UserEndpoint()
        {
            AuthenticationSettings authenticationSettings = new()
            {
                Facebook = new FacebookAuthenticationSettings
                {
                    AppId = AppId,
                },
            };
            IOptions<AuthenticationSettings> options = Options.Create(authenticationSettings);

            using (HttpClientTestingFactory http = new(HttpClientTestingFactorySettings))
            {
                FacebookAssertionGrantHandler handler = new(options, http.HttpClient);
                Task<AssertionGrantResult> resultTask = handler.ValidateAsync(Assertion);

                http.Expect(AppEndpoint).Respond(JsonConvert.SerializeObject(new FacebookApp { Id = AppId }));
                http.Expect(UserEndpoint).Respond(HttpStatusCode.BadRequest);

                AssertionGrantResult result = await resultTask;
                Assert.NotNull(result);
                Assert.False(result.IsSuccessful);

                http.EnsureNoOutstandingRequests();
            }
        }

        [Fact]
        public static async Task ValidateAsync_WrongApp()
        {
            AuthenticationSettings authenticationSettings = new()
            {
                Facebook = new FacebookAuthenticationSettings
                {
                    AppId = AppId,
                },
            };
            IOptions<AuthenticationSettings> options = Options.Create(authenticationSettings);

            using (HttpClientTestingFactory http = new(HttpClientTestingFactorySettings))
            {
                FacebookAssertionGrantHandler handler = new(options, http.HttpClient);
                Task<AssertionGrantResult> resultTask = handler.ValidateAsync(Assertion);

                http.Expect(AppEndpoint).Respond(JsonConvert.SerializeObject(new FacebookApp { Id = "SomeOtherAppId" }));

                AssertionGrantResult result = await resultTask;
                Assert.NotNull(result);
                Assert.False(result.IsSuccessful);

                http.EnsureNoOutstandingRequests();
            }
        }
    }
}
