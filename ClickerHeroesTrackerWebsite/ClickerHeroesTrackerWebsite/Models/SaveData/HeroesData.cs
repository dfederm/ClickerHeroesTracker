// <copyright file="HeroesData.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.SaveData
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Save game envelope for <see cref="HeroData"/>s.
    /// </summary>
    [JsonObject]
    public class HeroesData
    {
        /// <summary>
        /// Gets or sets a collection of <see cref="HeroData"/>.
        /// </summary>
        [JsonProperty(PropertyName = "heroes")]
        public IDictionary<int, HeroData> Heroes { get; set; }
    }
}