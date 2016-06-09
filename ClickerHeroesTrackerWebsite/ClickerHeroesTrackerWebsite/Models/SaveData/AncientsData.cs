// <copyright file="AncientsData.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.SaveData
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Save data envelope for the <see cref="AncientData"/>.
    /// </summary>
    [JsonObject]
    public class AncientsData
    {
        /// <summary>
        /// Gets or sets a collection of <see cref="AncientData"/>, keyed on ancient id.
        /// </summary>
        [JsonProperty(PropertyName = "ancients")]
        public IDictionary<int, AncientData> Ancients { get; set; }
    }
}