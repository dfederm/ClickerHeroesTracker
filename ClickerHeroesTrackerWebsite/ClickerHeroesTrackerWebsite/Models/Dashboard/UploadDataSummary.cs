namespace ClickerHeroesTrackerWebsite.Models.Dashboard
{
    using Models;
    using System;
    using System.Collections.Generic;

    public class UploadDataSummary
    {
        public UploadDataSummary(string userId)
        {
            const int NumUploads = 5;
            var uploads = new List<UploadData>(NumUploads);

            using (var command = new DatabaseCommand("GetUserUploads"))
            {
                command.AddParameter("@UserId", userId);
                command.AddParameter("@Offset", 0);
                command.AddParameter("@Count", NumUploads);

                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var uploadId = (int)reader["Id"];
                    var uploadTime = (DateTime)reader["UploadTime"];
                    var uploadData = new UploadData(uploadId, uploadTime);
                    uploads.Add(uploadData);
                }
            }

            this.Uploads = uploads;
            this.IsValid = uploads.Count > 0;
        }

        public bool IsValid { get; private set; }

        public IList<UploadData> Uploads { get; private set; }
    }
}