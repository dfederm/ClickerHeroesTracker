// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

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

namespace Website.Controllers
{
    /// <summary>
    /// This controller handles processing feedback.
    /// </summary>
    [Route("api/feedback")]
    [Authorize]
    [ApiController]
    public class FeedbackController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly TelemetryClient _telemetryClient;

        private readonly EmailSenderSettings _emailSenderSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedbackController"/> class.
        /// </summary>
        public FeedbackController(
            UserManager<ApplicationUser> userManager,
            TelemetryClient telemetryClient,
            IOptions<EmailSenderSettings> emailSenderSettingsOptions)
        {
            _userManager = userManager;
            _telemetryClient = telemetryClient;
            _emailSenderSettings = emailSenderSettingsOptions.Value;
        }

        /// <summary>
        /// Submits user feedback.
        /// </summary>
        /// <returns>An empty response with a status code representing the result.</returns>
        [Route("")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> SubmitAsync([FromForm] FeedbackRequest feedback)
        {
            string email = null;
            string userName = null;
            if (User.Identity.IsAuthenticated)
            {
                ApplicationUser user = await _userManager.GetUserAsync(User);
                email = await _userManager.GetEmailAsync(user);
                userName = user.UserName;
            }
            else if (!string.IsNullOrWhiteSpace(feedback.Email))
            {
                email = feedback.Email;
            }

            Dictionary<string, string> feedbackData = new()
            {
                { "Email", email ?? "<anonymous>" },
                { "UserName", userName ?? "<anonymous>" },
                { "Page", Request.Headers["Referer"] },
                { "Comments", feedback.Comments },
            };

            _telemetryClient.TrackEvent("FeedbackSubmit", feedbackData);

            SendGridClient client = new(_emailSenderSettings.ApiKey);

            SendGridMessage message = new()
            {
                From = new EmailAddress(email ?? "do-not-reply@clickerheroestracker.azurewebsites.net", userName ?? "Clicker Heroes Tracker"),
                Subject = $"Clicker Heroes Tracker Feedback from {userName ?? "<anonymous>"}",
                PlainTextContent = JsonConvert.SerializeObject(feedbackData, Formatting.Indented),
            };

            foreach (string feedbackReciever in _emailSenderSettings.FeedbackRecievers)
            {
                message.AddTo(new EmailAddress(feedbackReciever));
            }

            await client.SendEmailAsync(message);

            return Ok();
        }
    }
}
