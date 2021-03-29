// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace ClickerHeroesTrackerWebsite.Services.Email
{
    /// <summary>
    /// Service that sends emails.
    /// </summary>
    public class EmailSender : IEmailSender
    {
        private readonly EmailSenderSettings _emailSenderSettings;

        public EmailSender(IOptions<EmailSenderSettings> emailSenderSettingsOptions)
        {
            _emailSenderSettings = emailSenderSettingsOptions.Value;
        }

        /// <inheritdoc />
        public async Task SendEmailAsync(string email, string subject, string message)
        {
            SendGridClient client = new(_emailSenderSettings.ApiKey);

            SendGridMessage emailMessage = new()
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