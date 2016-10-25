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
    using ClickerHeroesTrackerWebsite.Models;
    using ClickerHeroesTrackerWebsite.Models.Feedback;
    using ClickerHeroesTrackerWebsite.Services.Email;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using SendGrid;
    using SendGrid.Helpers.Mail;

    /// <summary>
    /// This controller handles processing feedback.
    /// </summary>
    [Route("feedback")]
    public class FeedbackController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;

        private readonly TelemetryClient telemetryClient;

        private readonly EmailSenderSettings emailSenderSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedbackController"/> class.
        /// </summary>
        public FeedbackController(
            UserManager<ApplicationUser> userManager,
            TelemetryClient telemetryClient,
            IOptions<EmailSenderSettings> emailSenderSettingsOptions)
        {
            this.userManager = userManager;
            this.telemetryClient = telemetryClient;
            this.emailSenderSettings = emailSenderSettingsOptions.Value;
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
                var user = await this.userManager.GetUserAsync(this.User);
                email = await this.userManager.GetEmailAsync(user);
                userName = user.UserName;
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

            var client = new SendGridAPIClient(this.emailSenderSettings.ApiKey);

            var mail = new Mail();
            mail.From = new Email(email ?? "do-not-reply@clickerheroestracker.azurewebsites.net", userName ?? "Clicker Heroes Tracker");
            mail.Subject = $"Clicker Heroes Tracker Feedback from {userName ?? "<anonymous>"}";
            mail.AddContent(new Content("text/plain", JsonConvert.SerializeObject(feedbackData, Formatting.Indented)));
            var personalization = new Personalization();
            foreach (var feedbackReciever in this.emailSenderSettings.FeedbackRecievers)
            {
                personalization.AddTo(new Email(feedbackReciever));
            }

            mail.AddPersonalization(personalization);

            client.client.mail.send.post(requestBody: mail.Get());

            return this.Ok();
        }
    }
}
