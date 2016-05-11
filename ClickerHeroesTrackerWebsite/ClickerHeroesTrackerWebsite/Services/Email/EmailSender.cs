// <copyright file="EmailSender.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Services.Email
{
    using System.Net;
    using System.Net.Mail;
    using System.Threading.Tasks;
    using Microsoft.Extensions.OptionsModel;
    using SendGrid;

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
            var sendGridMessage = new SendGridMessage();
            sendGridMessage.AddTo(email);
            sendGridMessage.From = new MailAddress("do-not-reply@clickerheroestracker.azurewebsites.net", "Clicker Heroes Tracker");
            sendGridMessage.Subject = subject;
            sendGridMessage.Text = message;
            sendGridMessage.Html = message;

            var userName = this.emailSenderSettings.UserName;
            var password = this.emailSenderSettings.Password;
            var credentials = new NetworkCredential(
                userName,
                password);

            // Create a Web transport for sending email.
            var transportWeb = new Web(credentials);

            // Send the email.
            if (transportWeb != null)
            {
                return transportWeb.DeliverAsync(sendGridMessage);
            }
            else
            {
                return Task.FromResult(0);
            }
        }
    }
}