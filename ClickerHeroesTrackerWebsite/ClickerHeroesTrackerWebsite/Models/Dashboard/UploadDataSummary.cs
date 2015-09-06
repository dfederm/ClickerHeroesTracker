namespace ClickerHeroesTrackerWebsite.Models.Dashboard
{
    using Models;
    using System;
    using System.Collections.Generic;

    public class UploadDataSummary
    {
        public UploadDataSummary(string userId, UserSettings userSettings, int page, int count)
        {
            page = Math.Max(0, page);

            var uploads = new List<UploadData>(count);

            using (var command = new DatabaseCommand("GetUserUploads"))
            {
                command.AddParameter("@UserId", userId);
                command.AddParameter("@Offset", page * count);
                command.AddParameter("@Count", count);

                var returnParameter = command.AddReturnParameter();

                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var uploadId = Convert.ToInt32(reader["Id"]);
                    var uploadTime = Convert.ToDateTime(reader["UploadTime"]);
                    var uploadData = new UploadData(uploadId, uploadTime);
                    uploads.Add(uploadData);
                }

                // Move beyond the result above.
                reader.NextResult();

                this.TotalUploads = Convert.ToInt32(returnParameter.Value);
            }

            this.Uploads = uploads;
            this.Page = page;
            this.Count = count;
            this.UserSettings = userSettings;
            this.IsValid = uploads.Count > 0;
        }

        public bool IsValid { get; private set; }

        public int Page { get; private set; }

        public int Count { get; private set; }

        public int TotalUploads { get; private set; }

        public UserSettings UserSettings { get; private set; }

        public IList<UploadData> Uploads { get; private set; }
    }
}