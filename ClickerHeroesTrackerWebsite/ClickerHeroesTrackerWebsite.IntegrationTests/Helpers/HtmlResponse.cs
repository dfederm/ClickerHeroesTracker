// <copyright file="HtmlResponse.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.IntegrationTests.Helpers
{
    using System;
    using System.IO;
    using System.Net;

    internal sealed class HtmlResponse : IDisposable
    {
        private static string urlPrefix = GetUrlPrefix();

        private readonly HttpWebResponse response;

        private string content;

        public HtmlResponse(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(urlPrefix + url);
            try
            {
                this.response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException e)
            {
                this.response = (HttpWebResponse)e.Response;
            }
        }

        public HttpStatusCode StatusCode
        {
            get
            {
                return this.response.StatusCode;
            }
        }

        public string Content
        {
            get
            {
                return this.content = this.content = new StreamReader(this.response.GetResponseStream()).ReadToEnd();
            }
        }

        public void Dispose()
        {
            if (this.response != null)
            {
                this.response.Dispose();
            }
        }

        private static string GetUrlPrefix()
        {
            const string Protocol = "http://";
            string host;

            if (Environment.GetEnvironmentVariable("IsInCloud") == null)
            {
                host = "localhost:51083";
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

            return Protocol + host;
        }
    }
}
