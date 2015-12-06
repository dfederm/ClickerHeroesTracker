// <copyright file="ItemsData.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.SaveData
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    [JsonObject]
    public class ItemsData
    {
        [JsonProperty(PropertyName = "slots", Required = Required.Always)]
        public IDictionary<int, int> Slots { get; set; }

        [JsonProperty(PropertyName = "items", Required = Required.Always)]
        public IDictionary<int, ItemData> Items { get; set; }
    }
}