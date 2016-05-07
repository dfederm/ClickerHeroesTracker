// <copyright file="FeedbackTests.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.IntegrationTests
{
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Helpers;
    using Xunit;

    public class FeedbackTests
    {
        // Using the homepage as the feedback modal appears on every page.
        private const string Path = "/";

        private const string EmailInputLoggedIn = "<input type=\"email\" id=\"feedbackEmail\" name=\"email\" class=\"form-control\" value=\"Test User@test.com\" readonly=\"readonly\" />";

        private const string EmailInputAnonymous = "<input type=\"email\" id=\"feedbackEmail\" name=\"email\" class=\"form-control\" data-val=\"The email address was not valid.\" />";

        private const string EmailHelpTextAnonymous = "To allow Clicker Heroes Tracker to follow up with you using regarding this feedback, either log in or provide your email address.";

        [Fact]
        public async Task Feedback_Anonymous_BasicTest()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, Path);

            var response = await RequestManager.MakeRequest(request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.DoesNotContain(EmailInputLoggedIn, content);
            Assert.Contains(EmailInputAnonymous, content);
            Assert.Contains(EmailHelpTextAnonymous, content);
        }

        [Fact]
        public async Task Feedback_User_BasicTest()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, Path);
            request.AuthenticateUser();

            var response = await RequestManager.MakeRequest(request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains(EmailInputLoggedIn, content);
            Assert.DoesNotContain(EmailInputAnonymous, content);
            Assert.DoesNotContain(EmailHelpTextAnonymous, content);
        }
    }
}
