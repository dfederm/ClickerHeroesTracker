// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System.Collections.Generic;

namespace ClickerHeroesTrackerWebsite.Models.Api.Uploads
{
    /// <summary>
    /// Response data envelope for the upload list API.
    /// </summary>
    public sealed class UploadSummaryListResponse
    {
        /// <summary>
        /// Gets or sets the metadata for paging uploads.
        /// </summary>
        public PaginationMetadata Pagination { get; set; }

        /// <summary>
        /// Gets or sets the list of uploads requested.
        /// </summary>
        public IList<UploadSummary> Uploads { get; set; }
    }
}