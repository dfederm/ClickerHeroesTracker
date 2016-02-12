// <copyright file="NavbarTests.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.IntegrationTests
{
    using System.Net;
    using Helpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class NavbarTests
    {
        // Using the homepage as the navbar appears on every page.
        private const string Endpoint = "/";

        [TestMethod]
        public void Navbar_Anonymous_BasicTest()
        {
            using (var response = new HtmlResponse(Endpoint))
            {
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

                // Unauthenticated users should have "Register" and "Log in" links, but not an "Admin" link
                Assert.IsFalse(response.Content.Contains("Admin"));
                Assert.IsTrue(response.Content.Contains("Register"));
                Assert.IsTrue(response.Content.Contains("Log in"));
            }
        }

        [TestMethod]
        public void Navbar_User_BasicTest()
        {
            using (var response = new HtmlResponse(Endpoint, request => request.AuthenticateUser()))
            {
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

                // Authenticated users should have a "Welcome" and and "Log out" links, but not an "Admin" link
                Assert.IsFalse(response.Content.Contains("Admin"));
                Assert.IsTrue(response.Content.Contains("Hello Test User!"));
                Assert.IsTrue(response.Content.Contains("Log off"));
            }
        }

        [TestMethod]
        public void Navbar_Admin_BasicTest()
        {
            using (var response = new HtmlResponse(Endpoint, request => request.AuthenticateAdmin()))
            {
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

                // Admins should have a "Admin", "Welcome", and "Log out" links
                Assert.IsTrue(response.Content.Contains("Admin"));
                Assert.IsTrue(response.Content.Contains("Hello Test User!"));
                Assert.IsTrue(response.Content.Contains("Log off"));
            }
        }
    }
}
