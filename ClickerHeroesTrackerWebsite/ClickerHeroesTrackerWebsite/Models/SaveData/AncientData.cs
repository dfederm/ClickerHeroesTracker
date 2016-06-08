// <copyright file="AncientData.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.SaveData
{
    using Newtonsoft.Json;

    /// <summary>
    /// Save data for an ancient.
    /// </summary>
    [JsonObject]
    public class AncientData
    {
        /// <summary>
        /// Gets or sets the ancient id.
        /// </summary>
        [JsonProperty(PropertyName = "id", Required = Required.Always)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the ancient level.
        /// </summary>
        [JsonProperty(PropertyName = "level", Required = Required.Always)]
        public double Level { get; set; }

        /// <summary>
        /// Gets or sets the souls spent on this ancient.
        /// </summary>
        [JsonProperty(PropertyName = "spentHeroSouls", Required = Required.Always)]
        public double SpentHeroSouls { get; set; }
    }
}