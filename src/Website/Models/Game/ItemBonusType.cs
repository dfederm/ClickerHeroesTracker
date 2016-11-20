// <copyright file="ItemBonusType.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Game
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the data for an item bonus type in the game.
    /// </summary>
    [JsonObject]
    public sealed class ItemBonusType
    {
        /// <summary>
        /// Gets or sets the ancient id for this item bonus
        /// </summary>
        [JsonProperty(PropertyName = "ancientId", Required = Required.Always)]
        public int AncientId { get; set; }
    }
}
