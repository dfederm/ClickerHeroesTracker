// <copyright file="OutsidersData.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.SaveData
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Save data envelope for the <see cref="OutsiderData"/>.
    /// </summary>
    [JsonObject]
    public class OutsidersData
    {
        /// <summary>
        /// Gets or sets a collection of <see cref="OutsiderData"/>, keyed on outsider id.
        /// </summary>
        [JsonProperty(PropertyName = "outsiders")]
        public IDictionary<int, OutsiderData> Outsiders { get; set; }
    }
}