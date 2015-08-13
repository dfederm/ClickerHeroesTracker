namespace ClickerHeroesTrackerWebsite.Models.Dashboard
{
    using Microsoft.AspNet.Identity;
    using System;
    using System.Security.Principal;

    public class ProgressViewModel
    {
        public ProgressViewModel(IPrincipal user)
        {
            var userId = user.Identity.GetUserId();

            this.ProgressData = new ProgressData(userId, DateTime.UtcNow.AddDays(-7), null);

            this.IsValid = this.ProgressData.IsValid;
        }

        public bool IsValid { get; private set; }

        public ProgressData ProgressData { get; private set; }
    }
}