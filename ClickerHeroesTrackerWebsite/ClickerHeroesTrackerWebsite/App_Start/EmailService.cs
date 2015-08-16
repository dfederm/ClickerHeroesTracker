namespace ClickerHeroesTrackerWebsite
{

    using System.Configuration;

    using System.Net;
    using System.Net.Mail;
    using System.Threading.Tasks;
    using Microsoft.AspNet.Identity;
    using SendGrid;

    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            var myMessage = new SendGridMessage();
            myMessage.AddTo(message.Destination);
            myMessage.From = new MailAddress("do-not-reply@clickerheroestracker.azurewebsites.net", "Clicker Heroes Tracker");
            myMessage.Subject = message.Subject;
            myMessage.Text = message.Body;
            myMessage.Html = message.Body;

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
                return transportWeb.DeliverAsync(myMessage);
            }
            else
            {
                return Task.FromResult(0);
            }
        }
    }
}