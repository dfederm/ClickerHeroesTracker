// <copyright file="OutsiderData.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.SaveData
{
    using ClickerHeroesTrackerWebsite.Utility;
    using Newtonsoft.Json;

    /// <summary>
    /// Save data for an outsider.
    /// </summary>
    [JsonObject]
    public class OutsiderData
    {
        /// <summary>
        /// Gets or sets the outsider id.
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the outsider level.
        /// </summary>
        [JsonProperty(PropertyName = "level")]
        [JsonConverter(typeof(LongFloorConverter))]
        public long Level { get; set; }

        /// <summary>
        /// Gets or sets the ancient souls spent on this outsider.
        /// </summary>
        [JsonProperty(PropertyName = "spentAncientSouls")]
        [JsonConverter(typeof(LongFloorConverter))]
        public long SpentAncientSouls { get; set; }
    }
}