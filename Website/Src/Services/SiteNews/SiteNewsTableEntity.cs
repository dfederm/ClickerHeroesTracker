// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System;
using Microsoft.Azure.Cosmos.Table;

namespace Website.Services.SiteNews
{
    /// <summary>
    /// Represents an individual site news entity.
    /// </summary>
    public class SiteNewsTableEntity : TableEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SiteNewsTableEntity"/> class.
        /// </summary>
        /// <param name="date">The date of the entry.</param>
        /// <param name="order">The order of the entry.</param>
        public SiteNewsTableEntity(DateTime date, int order)
        {
            PartitionKey = date.ToString("yyyy-MM-dd");
            RowKey = order.ToString();
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
        /// Gets or sets the news entry message.
        /// </summary>
        public string Message { get; set; }
    }
}