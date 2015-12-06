// <copyright file="AncientsData.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.SaveData
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    [JsonObject]
    public class AncientsData
    {
        [JsonProperty(PropertyName = "ancients", Required = Required.Always)]
        public IDictionary<int, AncientData> Ancients { get; set; }
    }
}