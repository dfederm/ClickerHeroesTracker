// <copyright file="FeedbackTests.cs" company="Clicker Heroes Tracker">
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
    public class FeedbackTests
    {
        // Using the homepage as the feedback modal appears on every page.
        private const string Path = "/";

        private const string EmailInputLoggedIn = "<input type=\"email\" id=\"feedbackEmail\" name=\"email\" class=\"form-control\" value=\"Test User@test.com\" readonly=\"readonly\" />";

        private const string EmailInputAnonymous = "<input type=\"email\" id=\"feedbackEmail\" name=\"email\" class=\"form-control\" data-val=\"The email address was not valid.\" />";

        private const string EmailHelpTextAnonymous = "To allow Clicker Heroes Tracker to follow up with you using regarding this feedback, either log in or provide your email address.";

        [TestMethod]
        public async Task Feedback_Anonymous_BasicTest()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, Path);

            var response = await RequestManager.MakeRequest(request);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.IsFalse(content.Contains(EmailInputLoggedIn));
            Assert.IsTrue(content.Contains(EmailInputAnonymous));
            Assert.IsTrue(content.Contains(EmailHelpTextAnonymous));
        }

        [TestMethod]
        public async Task Feedback_User_BasicTest()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, Path);
            request.AuthenticateUser();

            var response = await RequestManager.MakeRequest(request);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(content.Contains(EmailInputLoggedIn));
            Assert.IsFalse(content.Contains(EmailInputAnonymous));
            Assert.IsFalse(content.Contains(EmailHelpTextAnonymous));
        }
    }
}
