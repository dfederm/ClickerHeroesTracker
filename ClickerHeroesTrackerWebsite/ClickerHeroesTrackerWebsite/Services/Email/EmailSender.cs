// <copyright file="EmailSender.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Services.Email
{
    using System.Threading.Tasks;
    using Microsoft.Extensions.Options;
    using SendGrid;
    using SendGrid.Helpers.Mail;

    /// <summary>
    /// Service that sends emails
    /// </summary>
    public class EmailSender : IEmailSender
    {
        private readonly EmailSenderSettings emailSenderSettings;

        public EmailSender(IOptions<EmailSenderSettings> emailSenderSettingsOptions)
        {
            this.emailSenderSettings = emailSenderSettingsOptions.Value;
        }

        /// <inheritdoc />
        public Task SendEmailAsync(string email, string subject, string message)
        {
            var client = new SendGridAPIClient(this.emailSenderSettings.ApiKey);

            var mail = new Mail(
                new Email("do-not-reply@clickerheroestracker.azurewebsites.net", "Clicker Heroes Tracker"),
                subject,
                new Email(email),
                new Content("text/html", message));

            client.client.mail.send.post(requestBody: mail.Get());

            return Task.CompletedTask;
        }
    }
}