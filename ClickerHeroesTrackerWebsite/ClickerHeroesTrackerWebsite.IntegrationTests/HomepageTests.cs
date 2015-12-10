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
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void Homepage_BasicTest()
        {
            using (var response = new HtmlResponse("/"))
            {
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            }
        }
    }
}
