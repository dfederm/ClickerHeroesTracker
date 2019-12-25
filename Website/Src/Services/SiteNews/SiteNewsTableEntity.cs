// <copyright file="SiteNewsTableEntity.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace Website.Services.SiteNews
{
    using System;
    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// Represents an individual site news entity.
    /// </summary>
    public class SiteNewsTableEntity : TableEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SiteNewsTableEntity"/> class.
        /// </summary>
        /// <param name="date">The date of the entry</param>
        /// <param name="order">The order of the entry</param>
        public SiteNewsTableEntity(DateTime date, int order)
        {
            this.PartitionKey = date.ToString("yyyy-MM-dd");
            this.RowKey = order.ToString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SiteNewsTableEntity"/> class.
        /// </summary>
        /// <remarks>
        /// Do not use. Internally used by the Azure Storage library.
        /// </remarks>
        public SiteNewsTableEntity()
        {
        }

        /// <summary>
        /// Gets or sets the news entry message
        /// </summary>
        public string Message { get; set; }
    }
}