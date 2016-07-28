// <copyright file="Ancient.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Game
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the data for an ancient in the game.
    /// </summary>
    [JsonObject]
    public class Ancient
    {
        private string name;

        /// <summary>
        /// Gets or sets the ancient id
        /// </summary>
        [JsonProperty(PropertyName = "id", Required = Required.Always)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the ancient's full name.
        /// </summary>
        [JsonProperty(PropertyName = "name", Required = Required.Always)]
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the ancient is non-transcendent (pre-1.0).
        /// </summary>
        [JsonProperty(PropertyName = "nonTranscendent")]
        public bool NonTranscendent { get; set; }

        /// <summary>
        /// Gets the ancient's name.
        /// </summary>
        [JsonIgnore]
        public string Name
        {
            get
            {
                if (this.name == null)
                {
                    var commaIndex = this.FullName.IndexOf(',');
                    this.name = commaIndex >= 0
                        ? this.FullName.Substring(0, commaIndex)
                        : this.FullName;
                }

                return this.name;
            }
        }
    }
}