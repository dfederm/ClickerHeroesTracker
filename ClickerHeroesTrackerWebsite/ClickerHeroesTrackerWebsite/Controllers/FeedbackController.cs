// <copyright file="FeedbackController.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Controllers
{
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Mail;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Mvc;
    using Microsoft.Extensions.Configuration;
    using Models;
    using Models.Feedback;
    using Newtonsoft.Json;
    using SendGrid;

    /// <summary>
    /// This controller handles processing feedback.
    /// </summary>
    [Route("feedback")]
    public class FeedbackController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;

        private readonly TelemetryClient telemetryClient;

        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedbackController"/> class.
        /// </summary>
        public FeedbackController(
            UserManager<ApplicationUser> userManager,
            TelemetryClient telemetryClient,
            IConfiguration configuration)
        {
            this.userManager = userManager;
            this.telemetryClient = telemetryClient;
            this.configuration = configuration;
        }

        /// <summary>
        /// Submits user feedback
        /// </summary>
        /// <returns>An empty response with a status code representing the result</returns>
        [Route("")]
        [HttpPost]
        public async Task<IActionResult> Submit(FeedbackRequest feedback)
        {
            string email = null;
            string userName = null;
            if (this.User.Identity.IsAuthenticated)
            {
                email = await this.userManager.GetEmailAsync(await userManager.FindByIdAsync(this.User.GetUserId()));
                userName = this.User.GetUserName();
            }
            else if (!string.IsNullOrWhiteSpace(feedback.Email))
            {
                email = feedback.Email;
            }

            var feedbackData = new Dictionary<string, string>
            {
                { "Email", email ?? "<anonymous>" },
                { "UserName", userName ?? "<anonymous>" },
                { "Page", this.Request.Headers["Referer"] },
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
                this.configuration["EmailSender:UserName"],
                this.configuration["EmailSender:Password"]);

            // Create a Web transport for sending email.
            var transportWeb = new Web(credentials);

            // Send the email.
            await transportWeb.DeliverAsync(sendGridMessage);

            return this.Ok();
        }
    }
}
