// <copyright file="FeedbackController.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace Website.Controllers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ClickerHeroesTrackerWebsite.Models;
    using ClickerHeroesTrackerWebsite.Services.Email;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using SendGrid;
    using SendGrid.Helpers.Mail;
    using Website.Models.Api.Feedback;

    /// <summary>
    /// This controller handles processing feedback.
    /// </summary>
    [Route("api/feedback")]
    [Authorize]
    [ApiController]
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
        [AllowAnonymous]
        public async Task<ActionResult> Submit([FromForm] FeedbackRequest feedback)
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

            var client = new SendGridClient(this.emailSenderSettings.ApiKey);

            var message = new SendGridMessage
            {
                From = new EmailAddress(email ?? "do-not-reply@clickerheroestracker.azurewebsites.net", userName ?? "Clicker Heroes Tracker"),
                Subject = $"Clicker Heroes Tracker Feedback from {userName ?? "<anonymous>"}",
                PlainTextContent = JsonConvert.SerializeObject(feedbackData, Formatting.Indented),
            };

            foreach (var feedbackReciever in this.emailSenderSettings.FeedbackRecievers)
            {
                message.AddTo(new EmailAddress(feedbackReciever));
            }

            await client.SendEmailAsync(message);

            return this.Ok();
        }
    }
}
