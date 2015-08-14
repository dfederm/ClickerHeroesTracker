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

            var userSettings = new UserSettings(userId);
            userSettings.Fill();

            var startTime = DateTime.UtcNow.AddDays(-7);
            using (var command = new DatabaseCommand("GetProgressData"))
            {
                command.AddParameter("@UserId", userId);
                command.AddParameter("@StartTime", startTime);

                var reader = command.ExecuteReader();

                this.ProgressData = new ProgressData(reader, userSettings);
            }

            this.IsValid = this.ProgressData.IsValid;
        }

        public bool IsValid { get; private set; }

        public ProgressData ProgressData { get; private set; }
    }
}