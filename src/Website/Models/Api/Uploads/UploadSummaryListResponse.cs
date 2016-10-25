// <copyright file="UploadSummaryListResponse.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Api.Uploads
{
    using System.Collections.Generic;

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
        public IList<Upload> Uploads { get; set; }
    }
}