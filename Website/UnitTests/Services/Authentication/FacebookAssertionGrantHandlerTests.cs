// <copyright file="FacebookAssertionGrantHandlerTests.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace UnitTests.Services.Authentication
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Testing.HttpClient;
    using Website.Models.Authentication;
    using Website.Services.Authentication;
    using Xunit;

    public static class FacebookAssertionGrantHandlerTests
    {
        private const string AppId = "SomeAppId";
        private const string ExternalUserId = "SomeExternalUserId";
        private const string ExternalUserEmail = "SomeExternalUserEmail";
        private const string Assertion = "SomeAssertion";
        private const string AppEndpoint = "https://graph.facebook.com/app/?access_token=" + Assertion;
        private const string UserEndpoint = "https://graph.facebook.com/me?fields=id,email&access_token=" + Assertion;

        // Upping the match timeout since this makes sequential http calls which for some reason may take more than the default of 100ms to chain together.
        private static readonly HttpClientTestingFactorySettings HttpClientTestingFactorySettings = new HttpClientTestingFactorySettings { ExpectationMatchTimeout = TimeSpan.FromSeconds(1) };

        [Fact]
        public static async Task ValidateAsync_Success()
        {
            var authenticationSettings = new AuthenticationSettings
            {
                Facebook = new FacebookAuthenticationSettings
                {
                    AppId = AppId,
                },
            };
            var options = Options.Create(authenticationSettings);

            using (var http = new HttpClientTestingFactory(HttpClientTestingFactorySettings))
            {
                var handler = new FacebookAssertionGrantHandler(options, http.HttpClient);
                var resultTask = handler.ValidateAsync(Assertion);

                http.Expect(AppEndpoint).Respond(JsonConvert.SerializeObject(new FacebookApp { Id = AppId }));
                http.Expect(UserEndpoint).Respond(JsonConvert.SerializeObject(new FacebookUser { Id = ExternalUserId, Email = ExternalUserEmail }));

                var result = await resultTask;
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
            var authenticationSettings = new AuthenticationSettings();
            var options = Options.Create(authenticationSettings);

            using (var http = new HttpClientTestingFactory(HttpClientTestingFactorySettings))
            {
                var handler = new FacebookAssertionGrantHandler(options, http.HttpClient);
                var resultTask = handler.ValidateAsync(Assertion);

                http.Expect(AppEndpoint).Respond(HttpStatusCode.BadRequest);

                var result = await resultTask;
                Assert.NotNull(result);
                Assert.False(result.IsSuccessful);

                http.EnsureNoOutstandingRequests();
            }
        }

        [Fact]
        public static async Task ValidateAsync_HttpError_UserEndpoint()
        {
            var authenticationSettings = new AuthenticationSettings
            {
                Facebook = new FacebookAuthenticationSettings
                {
                    AppId = AppId,
                },
            };
            var options = Options.Create(authenticationSettings);

            using (var http = new HttpClientTestingFactory(HttpClientTestingFactorySettings))
            {
                var handler = new FacebookAssertionGrantHandler(options, http.HttpClient);
                var resultTask = handler.ValidateAsync(Assertion);

                http.Expect(AppEndpoint).Respond(JsonConvert.SerializeObject(new FacebookApp { Id = AppId }));
                http.Expect(UserEndpoint).Respond(HttpStatusCode.BadRequest);

                var result = await resultTask;
                Assert.NotNull(result);
                Assert.False(result.IsSuccessful);

                http.EnsureNoOutstandingRequests();
            }
        }

        [Fact]
        public static async Task ValidateAsync_WrongApp()
        {
            var authenticationSettings = new AuthenticationSettings
            {
                Facebook = new FacebookAuthenticationSettings
                {
                    AppId = AppId,
                },
            };
            var options = Options.Create(authenticationSettings);

            using (var http = new HttpClientTestingFactory(HttpClientTestingFactorySettings))
            {
                var handler = new FacebookAssertionGrantHandler(options, http.HttpClient);
                var resultTask = handler.ValidateAsync(Assertion);

                http.Expect(AppEndpoint).Respond(JsonConvert.SerializeObject(new FacebookApp { Id = "SomeOtherAppId" }));

                var result = await resultTask;
                Assert.NotNull(result);
                Assert.False(result.IsSuccessful);

                http.EnsureNoOutstandingRequests();
            }
        }
    }
}
