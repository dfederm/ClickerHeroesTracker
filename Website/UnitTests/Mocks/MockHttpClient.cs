// <copyright file="MockHttpClient.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace UnitTests.Mocks
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Xunit;

    internal sealed class MockHttpClient : HttpClient
    {
        private readonly MockResponseHandler handler;

        public MockHttpClient()
            : this(new MockResponseHandler())
        {
        }

        private MockHttpClient(MockResponseHandler handler)
            : base(handler)
        {
            this.handler = handler;
        }

        public void AddMockResponse(string url, object responseObject)
        {
            this.AddMockResponse(url, JsonConvert.SerializeObject(responseObject));
        }

        public void AddMockResponse(string url, string content)
        {
            var uri = new Uri(url);
            var response = new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(content) };
            this.AddMockResponse(uri, response);
        }

        public void AddMockResponse(string url, HttpStatusCode statusCode)
        {
            var uri = new Uri(url);
            var response = new HttpResponseMessage { StatusCode = statusCode };
            this.AddMockResponse(uri, response);
        }

        public void AddMockResponse(string url, HttpResponseMessage response)
        {
            var uri = new Uri(url);
            this.AddMockResponse(uri, response);
        }

        public void AddMockResponse(Uri uri, HttpResponseMessage response)
        {
            this.handler.MockResponses.Add(uri, response);
        }

        public void VerifyNoOutstandingRequests()
        {
            Assert.True(this.handler.MockResponses.Count == 0, $"Outstanding requests:{Environment.NewLine}{string.Join(Environment.NewLine, this.handler.MockResponses.Keys)}");
        }

        private sealed class MockResponseHandler : DelegatingHandler
        {
            public Dictionary<Uri, HttpResponseMessage> MockResponses { get; } = new Dictionary<Uri, HttpResponseMessage>();

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
            {
                Assert.True(this.MockResponses.TryGetValue(request.RequestUri, out var response), $"Unexpected request: {request.Method} {request.RequestUri}");
                this.MockResponses.Remove(request.RequestUri);
                return Task.FromResult(response);
            }
        }
    }
}
