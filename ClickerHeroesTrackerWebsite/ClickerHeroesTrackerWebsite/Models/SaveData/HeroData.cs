// <copyright file="HeroData.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.SaveData
{
    using Newtonsoft.Json;

    [JsonObject]
    public class HeroData
    {
        [JsonProperty(PropertyName = "id", Required = Required.Always)]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "level", Required = Required.Always)]
        public int Level { get; set; }

        [JsonProperty(PropertyName = "epicLevel", Required = Required.Always)]
        public int Gilds { get; set; }
    }
}