// <copyright file="Ancient.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Game
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the data for an ancient in the game.
    /// </summary>
    [JsonObject]
    public class Ancient
    {
        private string name;

        /// <summary>
        /// Gets or sets the max level for the ancient.
        /// </summary>
        [JsonProperty(PropertyName = "maxLevel", Required = Required.Always)]
        public int MaxLevel { get; set; }

        /// <summary>
        /// Gets or sets the flavor text for the ancient.
        /// </summary>
        [JsonProperty(PropertyName = "flavorText", Required = Required.Always)]
        public string FlavorText { get; set; }

        /// <summary>
        /// Gets or sets the parameter name
        /// </summary>
        /// <remarks>
        /// This has no known use.
        /// </remarks>
        [JsonProperty(PropertyName = "paramName", Required = Required.Always)]
        public string ParameterName { get; set; }

        /// <summary>
        /// Gets or sets the level amount formula.
        /// </summary>
        [JsonProperty(PropertyName = "levelAmountFormula", Required = Required.Always)]
        public LevelAmountFormula LevelAmountFormula { get; set; }

        /// <summary>
        /// Gets or sets the level cost formula.
        /// </summary>
        [JsonProperty(PropertyName = "levelCostFormula", Required = Required.Always)]
        public LevelCostFormula LevelCostFormula { get; set; }

        /// <summary>
        /// Gets or sets the ancient description
        /// </summary>
        [JsonProperty(PropertyName = "effectDescription", Required = Required.Always)]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the ancient id
        /// </summary>
        [JsonProperty(PropertyName = "id", Required = Required.Always)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the id of the icon to use for this ancient.
        /// </summary>
        [JsonProperty(PropertyName = "iconId", Required = Required.Always)]
        public int IconId { get; set; }

        /// <summary>
        /// Gets or sets the ancient's full name.
        /// </summary>
        [JsonProperty(PropertyName = "name", Required = Required.Always)]
        public string FullName { get; set; }

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