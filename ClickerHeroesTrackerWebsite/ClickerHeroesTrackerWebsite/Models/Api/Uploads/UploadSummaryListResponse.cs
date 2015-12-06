// <copyright file="UploadSummaryListResponse.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Api.Uploads
{
    using System.Collections.Generic;

    public sealed class UploadSummaryListResponse
    {
        public PaginationMetadata Pagination { get; set; }

        public IList<UploadSummary> Uploads { get; set; }
    }
}