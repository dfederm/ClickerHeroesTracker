// <copyright file="HomepageTests.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.IntegrationTests
{
    using System.Net;
    using Helpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class HomepageTests
    {
        private const string Endpoint = "/";

        [TestMethod]
        public void Homepage_Anonymous_BasicTest()
        {
            using (var response = new HtmlResponse(Endpoint))
            {
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

                Assert.IsTrue(response.Content.Contains("jumbotron"));
            }
        }

        [TestMethod]
        public void Homepage_User_BasicTest()
        {
            using (var response = new HtmlResponse(Endpoint, request => request.AuthenticateUser()))
            {
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

                Assert.IsTrue(response.Content.Contains("jumbotron"));
            }
        }
    }
}
