// <copyright file="FeedbackTests.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.IntegrationTests
{
    using System.Net;
    using Helpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class FeedbackTests
    {
        // Using the homepage as the feedback modal appears on every page.
        private const string Endpoint = "/";

        [TestMethod]
        public void Feedback_Anonymous_BasicTest()
        {
            using (var response = new HtmlResponse(Endpoint))
            {
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

                Assert.IsFalse(response.Content.Contains("Allow Clicker Heroes Tracker to follow up with you using your email \"Test User@test.com\" regarding this feedback."));
                Assert.IsTrue(response.Content.Contains("To allow Clicker Heroes Tracker to follow up with you using regarding this feedback, either log in or provide your email address."));
            }
        }

        [TestMethod]
        public void Feedback_User_BasicTest()
        {
            using (var response = new HtmlResponse(Endpoint, request => request.AuthenticateUser()))
            {
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

                Assert.IsTrue(response.Content.Contains("Allow Clicker Heroes Tracker to follow up with you using your email \"Test User@test.com\" regarding this feedback."));
                Assert.IsFalse(response.Content.Contains("To allow Clicker Heroes Tracker to follow up with you using regarding this feedback, either log in or provide your email address."));
            }
        }
    }
}
