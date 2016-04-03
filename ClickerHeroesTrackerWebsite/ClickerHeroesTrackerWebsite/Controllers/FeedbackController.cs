// <copyright file="FeedbackController.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Controllers
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Net;
    using System.Net.Http;
    using System.Net.Mail;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.Owin;
    using Models.Feedback;
    using Newtonsoft.Json;
    using SendGrid;

    /// <summary>
    /// This controller handles processing feedback.
    /// </summary>
    [RoutePrefix("feedback")]
    public class FeedbackController : ApiController
    {
        private readonly TelemetryClient telemetryClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedbackController"/> class.
        /// </summary>
        public FeedbackController(TelemetryClient telemetryClient)
        {
            this.telemetryClient = telemetryClient;
        }

        /// <summary>
        /// Submits user feedback
        /// </summary>
        /// <returns>An empty response with a status code representing the result</returns>
        [Route("")]
        [HttpPost]
        public async Task<HttpResponseMessage> Submit(FeedbackRequest feedback)
        {
            string email = null;
            string userName = null;
            if (this.User.Identity.IsAuthenticated)
            {
                email = this.Request
                    .GetOwinContext()
                    .GetUserManager<ApplicationUserManager>()
                    .GetEmail(this.User.Identity.GetUserId());
                userName = this.User.Identity.GetUserName();
            }
            else if (!string.IsNullOrWhiteSpace(feedback.Email))
            {
                email = feedback.Email;
            }

            var feedbackData = new Dictionary<string, string>
            {
                { "Email", email ?? "<anonymous>" },
                { "UserName", userName ?? "<anonymous>" },
                { "Page", this.Request.Headers.Referrer.AbsoluteUri },
                { "Comments", feedback.Comments },
            };

            this.telemetryClient.TrackEvent("FeedbackSubmit", feedbackData);

            var sendGridMessage = new SendGridMessage();
            sendGridMessage.AddTo("david.federman@outlook.com");
            sendGridMessage.AddTo("ender336@gmail.com");
            sendGridMessage.From = new MailAddress(email ?? "do-not-reply@clickerheroestracker.azurewebsites.net", userName ?? "Clicker Heroes Tracker");
            sendGridMessage.Subject = $"Clicker Heroes Tracker Feedback from {userName ?? "<anonymous>"}";
            sendGridMessage.Text = JsonConvert.SerializeObject(feedbackData, Formatting.Indented);

            var credentials = new NetworkCredential(
                ConfigurationManager.AppSettings.Get("SendGrid_UserName"),
                ConfigurationManager.AppSettings.Get("SendGrid_Password"));

            // Create a Web transport for sending email.
            var transportWeb = new Web(credentials);

            // Send the email.
            await transportWeb.DeliverAsync(sendGridMessage);

            return this.Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}
