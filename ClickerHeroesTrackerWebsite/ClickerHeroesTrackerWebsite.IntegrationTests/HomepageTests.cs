// <copyright file="HomepageTests.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.IntegrationTests
{
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Helpers;
    using Xunit;

    public class HomepageTests
    {
        private const string Path = "/";

        [Fact]
        public async Task Homepage_Anonymous_BasicTest()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, Path);

            var response = await RequestManager.MakeRequest(request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.True(content.Contains("jumbotron"));
        }

        [Fact]
        public async Task Homepage_User_BasicTest()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, Path);
            request.AuthenticateUser();

            var response = await RequestManager.MakeRequest(request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.True(content.Contains("jumbotron"));
        }
    }
}
