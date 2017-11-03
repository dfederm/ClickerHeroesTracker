// <copyright file="NavbarTests.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.IntegrationTests
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Helpers;
    using Xunit;

    public class NavbarTests
    {
        // Using the homepage as the navbar appears on every page.
        private const string Path = "/";

        [Fact]
        public async Task Navbar_Anonymous_BasicTest()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, Path);

            var response = await RequestManager.MakeRequest(request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Unauthenticated users should have "Register" and "Log in" links, but not an "Admin" link
            var content = await response.Content.ReadAsStringAsync();
            Assert.DoesNotContain("Admin", content, StringComparison.Ordinal);
            Assert.Contains("Register", content, StringComparison.Ordinal);
            Assert.Contains("Log in", content, StringComparison.Ordinal);
        }

        [Fact]
        public async Task Navbar_User_BasicTest()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, Path);
            request.AuthenticateUser();

            var response = await RequestManager.MakeRequest(request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Authenticated users should have a "Welcome" and and "Log out" links, but not an "Admin" link
            var content = await response.Content.ReadAsStringAsync();
            Assert.DoesNotContain("Admin", content, StringComparison.Ordinal);
            Assert.Contains("Hello Test User!", content, StringComparison.Ordinal);
            Assert.Contains("Log off", content, StringComparison.Ordinal);
        }
    }
}
