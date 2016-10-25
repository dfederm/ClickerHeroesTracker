// <copyright file="ItemsData.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.SaveData
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Save game data for a user's items.
    /// </summary>
    [JsonObject]
    public class ItemsData
    {
        /// <summary>
        /// Gets or sets a mapping of slot number to item number.
        /// </summary>
        [JsonProperty(PropertyName = "slots")]
        public IDictionary<int, int> Slots { get; set; }

        /// <summary>
        /// Gets or sets a mapping of item number to the item data.
        /// </summary>
        [JsonProperty(PropertyName = "items")]
        public IDictionary<int, ItemData> Items { get; set; }
    }
}