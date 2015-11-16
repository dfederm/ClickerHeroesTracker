namespace ClickerHeroesTrackerWebsite.Models.Api.Uploads
{
    using System.Collections.Generic;

    public sealed class UploadSummaryListResponse
    {
        public PaginationMetadata Pagination { get; set; }

        public IList<UploadSummary> Uploads { get; set; }
    }
}