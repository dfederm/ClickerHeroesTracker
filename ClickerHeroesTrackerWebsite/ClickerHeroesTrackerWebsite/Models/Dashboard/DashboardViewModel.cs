namespace ClickerHeroesTrackerWebsite.Models.Dashboard
{
    using Microsoft.AspNet.Identity;
    using System;
    using System.Security.Principal;

    public class DashboardViewModel
    {
        public DashboardViewModel(IPrincipal user)
        {
            var userId = user.Identity.GetUserId();
            var startTime = DateTime.UtcNow.AddDays(-7);

            this.UploadDataSummary = new UploadDataSummary(userId);

            using (var command = new DatabaseCommand("GetProgressData"))
            {
                command.AddParameter("@UserId", userId);
                command.AddParameter("@StartTime", startTime);

                var reader = command.ExecuteReader();

                this.ProgressData = new ProgressData(reader);
            }

            this.RivalDataList = new RivalDataList(userId);

            this.IsValid = this.UploadDataSummary.IsValid && this.ProgressData.IsValid;
        }

        public bool IsValid { get; private set; }

        public UploadDataSummary UploadDataSummary { get; private set; }

        public ProgressData ProgressData { get; private set; }

        public RivalDataList RivalDataList { get; private set; }
    }
}