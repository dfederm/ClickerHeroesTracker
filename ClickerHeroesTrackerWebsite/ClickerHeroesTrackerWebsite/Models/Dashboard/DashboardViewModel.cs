namespace ClickerHeroesTrackerWebsite.Models.Dashboard
{
    using Microsoft.AspNet.Identity;
    using System.Security.Principal;

    public class DashboardViewModel
    {
        public DashboardViewModel(IPrincipal user)
        {
            var userId = user.Identity.GetUserId();

            this.UploadDataSummary = new UploadDataSummary(userId);
            this.ProgressData = new ProgressData(userId);
        }

        public UploadDataSummary UploadDataSummary { get; private set; }

        public ProgressData ProgressData { get; private set; }
    }
}