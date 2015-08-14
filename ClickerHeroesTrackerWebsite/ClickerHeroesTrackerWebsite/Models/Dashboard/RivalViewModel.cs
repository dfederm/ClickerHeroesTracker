namespace ClickerHeroesTrackerWebsite.Models.Dashboard
{
    using Microsoft.AspNet.Identity;
    using System;
    using System.Security.Principal;

    public class RivalViewModel
    {
        public RivalViewModel(IPrincipal user, int rivalId)
        {
            var userId = user.Identity.GetUserId();

            var userSettings = new UserSettings(userId);
            userSettings.Fill();

            var startTime = DateTime.UtcNow.AddDays(-7);
            using (var command = new DatabaseCommand("GetRivalData"))
            {
                command.AddParameter("@UserId", userId);
                command.AddParameter("@RivalId", rivalId);
                command.AddParameter("@StartTime", startTime);

                var reader = command.ExecuteReader();

                this.UserProgressData = new ProgressData(reader, userSettings);
                if (!reader.NextResult())
                {
                    return;
                }

                if (reader.Read())
                {
                    this.RivalUserName = (string)reader["RivalUserName"];
                }
                else
                {
                    return;
                }

                if (!reader.NextResult())
                {
                    return;
                }

                this.RivalProgressData = new ProgressData(reader, userSettings);
            }

            this.IsValid = !string.IsNullOrEmpty(this.RivalUserName)
                && this.UserProgressData.IsValid
                && this.RivalProgressData.IsValid;
        }

        public bool IsValid { get; private set; }

        public string RivalUserName { get; private set; }

        public ProgressData UserProgressData { get; private set; }

        public ProgressData RivalProgressData { get; private set; }
    }
}