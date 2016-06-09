// <copyright file="OutsiderData.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.SaveData
{
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
        public double Level { get; set; }

        /// <summary>
        /// Gets or sets the ancient souls spent on this outsider.
        /// </summary>
        [JsonProperty(PropertyName = "spentAncientSouls")]
        public double SpentAncientSouls { get; set; }
    }
}