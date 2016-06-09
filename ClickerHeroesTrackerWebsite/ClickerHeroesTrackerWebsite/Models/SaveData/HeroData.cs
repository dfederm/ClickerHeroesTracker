// <copyright file="HeroData.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.SaveData
{
    using Newtonsoft.Json;

    /// <summary>
    /// Save data for a hero
    /// </summary>
    [JsonObject]
    public class HeroData
    {
        /// <summary>
        /// Gets or sets the hero id
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the hero level
        /// </summary>
        [JsonProperty(PropertyName = "level")]
        public int Level { get; set; }

        /// <summary>
        /// Gets or sets the hero gilds
        /// </summary>
        [JsonProperty(PropertyName = "epicLevel")]
        public int Gilds { get; set; }
    }
}