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
        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var client = new SendGridClient(this.emailSenderSettings.ApiKey);

            var emailMessage = new SendGridMessage
            {
                From = new EmailAddress("do-not-reply@clickerheroestracker.azurewebsites.net", "Clicker Heroes Tracker"),
                Subject = subject,
                HtmlContent = message,
            };
            emailMessage.AddTo(new EmailAddress(email));

            await client.SendEmailAsync(emailMessage);
        }
    }
}