// <copyright file="EmailService.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite
{
    using System.Configuration;
    using System.Net;
    using System.Net.Mail;
    using System.Threading.Tasks;
    using Microsoft.AspNet.Identity;
    using SendGrid;

    /// <summary>
    /// Service that sends emails
    /// </summary>
    public class EmailService : IIdentityMessageService
    {
        /// <inheritdoc />
        public Task SendAsync(IdentityMessage message)
        {
            var sendGridMessage = new SendGridMessage();
            sendGridMessage.AddTo(message.Destination);
            sendGridMessage.From = new MailAddress("do-not-reply@clickerheroestracker.azurewebsites.net", "Clicker Heroes Tracker");
            sendGridMessage.Subject = message.Subject;
            sendGridMessage.Text = message.Body;
            sendGridMessage.Html = message.Body;

            var userName = ConfigurationManager.AppSettings.Get("SendGrid_UserName");
            var password = ConfigurationManager.AppSettings.Get("SendGrid_Password");
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