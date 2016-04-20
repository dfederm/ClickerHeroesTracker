// <copyright file="RequestManager.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.IntegrationTests.Helpers
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Net.Http;
    using System.Threading.Tasks;

    internal static class RequestManager
    {
        private static Uri baseUri = GetBaseUri();

        [SuppressMessage("Microsoft.Usage", "CA2213", Justification = "The intent is for the client to last the entire process. It will disposed upon destruction at app teardown.")]
        private static HttpClient httpClient = new HttpClient();

        public static async Task<HttpResponseMessage> MakeRequest(HttpRequestMessage request)
        {
            // Root the request
            request.RequestUri = new Uri(baseUri, request.RequestUri);

            return await httpClient.SendAsync(request);
        }

        private static Uri GetBaseUri()
        {
            const string Protocol = "http://";
            string host;

            if (Environment.GetEnvironmentVariable("IsInCloud") == null)
            {
                host = "localhost:5000";
            }
            else
            {
                var websiteName = Environment.GetEnvironmentVariable("WebsiteName");
                var slot = Environment.GetEnvironmentVariable("Slot");

                host = websiteName;
                if (slot != null)
                {
                    host += "-" + slot;
                }

                host += ".azurewebsites.net";
            }

            return new Uri(Protocol + host);
        }
    }
}
