// <copyright file="NavbarTests.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.IntegrationTests
{
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Helpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class NavbarTests
    {
        // Using the homepage as the navbar appears on every page.
        private const string Path = "/";

        [TestMethod]
        public async Task Navbar_Anonymous_BasicTest()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, Path);

            var response = await RequestManager.MakeRequest(request);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            // Unauthenticated users should have "Register" and "Log in" links, but not an "Admin" link
            var content = await response.Content.ReadAsStringAsync();
            Assert.IsFalse(content.Contains("Admin"));
            Assert.IsTrue(content.Contains("Register"));
            Assert.IsTrue(content.Contains("Log in"));
        }

        [TestMethod]
        public async Task Navbar_User_BasicTest()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, Path);
            request.AuthenticateUser();

            var response = await RequestManager.MakeRequest(request);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            // Authenticated users should have a "Welcome" and and "Log out" links, but not an "Admin" link
            var content = await response.Content.ReadAsStringAsync();
            Assert.IsFalse(content.Contains("Admin"));
            Assert.IsTrue(content.Contains("Hello Test User!"));
            Assert.IsTrue(content.Contains("Log off"));
        }

        [TestMethod]
        public async Task Navbar_Admin_BasicTest()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, Path);
            request.AuthenticateAdmin();

            var response = await RequestManager.MakeRequest(request);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            // Admins should have a "Admin", "Welcome", and "Log out" links
            var content = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(content.Contains("Admin"));
            Assert.IsTrue(content.Contains("Hello Test User!"));
            Assert.IsTrue(content.Contains("Log off"));
        }
    }
}
