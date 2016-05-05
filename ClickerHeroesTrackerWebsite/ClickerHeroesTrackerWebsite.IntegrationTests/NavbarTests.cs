// <copyright file="NavbarTests.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.IntegrationTests
{
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
            Assert.False(content.Contains("Admin"));
            Assert.True(content.Contains("Register"));
            Assert.True(content.Contains("Log in"));
        }

        [Fact(Skip = "BUGBUG 45: Ignoring until we can properly mock the database")]
        public async Task Navbar_User_BasicTest()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, Path);
            request.AuthenticateUser();

            var response = await RequestManager.MakeRequest(request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Authenticated users should have a "Welcome" and and "Log out" links, but not an "Admin" link
            var content = await response.Content.ReadAsStringAsync();
            Assert.False(content.Contains("Admin"));
            Assert.True(content.Contains("Hello Test User!"));
            Assert.True(content.Contains("Log off"));
        }

        [Fact(Skip = "BUGBUG 45: Ignoring until we can properly mock the database")]
        public async Task Navbar_Admin_BasicTest()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, Path);
            request.AuthenticateAdmin();

            var response = await RequestManager.MakeRequest(request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Admins should have a "Admin", "Welcome", and "Log out" links
            var content = await response.Content.ReadAsStringAsync();
            Assert.True(content.Contains("Admin"));
            Assert.True(content.Contains("Hello Test User!"));
            Assert.True(content.Contains("Log off"));
        }
    }
}
