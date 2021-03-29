// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System;
using Newtonsoft.Json;

namespace ClickerHeroesTrackerWebsite.Models.Game
{
    /// <summary>
    /// Represents the data for an ancient in the game.
    /// </summary>
    [JsonObject]
    public class Ancient
    {
        private string _name;

        /// <summary>
        /// Gets or sets the ancient id.
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
        [JsonProperty(PropertyName = "shortName")]
        public string Name
        {
            get
            {
                if (_name == null)
                {
                    int commaIndex = FullName.IndexOf(',', StringComparison.Ordinal);
                    _name = commaIndex >= 0
                        ? FullName.Substring(0, commaIndex)
                        : FullName;
                }

                return _name;
            }
        }
    }
}