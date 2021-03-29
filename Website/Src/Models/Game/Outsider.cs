// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using Newtonsoft.Json;

namespace ClickerHeroesTrackerWebsite.Models.Game
{
    /// <summary>
    /// Represents the data for an outsider in the game.
    /// </summary>
    [JsonObject]
    public class Outsider
    {
        /// <summary>
        /// Gets or sets the outsider id.
        /// </summary>
        [JsonProperty(PropertyName = "id", Required = Required.Always)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the outsider's name.
        /// </summary>
        [JsonProperty(PropertyName = "name", Required = Required.Always)]
        public string Name { get; set; }
    }
}