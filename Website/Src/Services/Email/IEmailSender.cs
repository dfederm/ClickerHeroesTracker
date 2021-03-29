// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System.Threading.Tasks;

namespace ClickerHeroesTrackerWebsite.Services.Email
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
