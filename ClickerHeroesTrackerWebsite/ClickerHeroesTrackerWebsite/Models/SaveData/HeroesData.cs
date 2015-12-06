// <copyright file="HeroesData.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.SaveData
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    [JsonObject]
    public class HeroesData
    {
        [JsonProperty(PropertyName = "heroes", Required = Required.Always)]
        public IDictionary<int, HeroData> Heroes { get; set; }
    }
}