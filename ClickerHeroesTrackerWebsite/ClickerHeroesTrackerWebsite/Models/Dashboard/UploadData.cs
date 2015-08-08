namespace ClickerHeroesTrackerWebsite.Models.Dashboard
{
    using System;

    public class UploadData
    {
        public UploadData(int uploadId, DateTime uploadTime)
        {
            this.UploadId = uploadId;
            this.UploadTime = uploadTime;
        }

        public int UploadId { get; private set; }

        public DateTime UploadTime { get; private set; }
    }
}