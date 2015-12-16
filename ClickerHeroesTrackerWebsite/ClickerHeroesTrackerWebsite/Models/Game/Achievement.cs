// <copyright file="Achievement.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Game
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Utility;

    /// <summary>
    /// Represents an achievement in the game
    /// </summary>
    [JsonObject]
    public class Achievement
    {
        /// <summary>
        /// Gets or sets the parameters to pass to the <see cref="RewardFunction"/>.
        /// </summary>
        [JsonProperty(PropertyName = "rewardParams", Required = Required.Always)]
        [JsonConverter(typeof(StringDoubleListConverter))]
        public double[] RewardFunctionParams { get; set; }

        /// <summary>
        /// Gets or sets the achievement reward text.
        /// </summary>
        [JsonProperty(PropertyName = "rewardText", Required = Required.Always)]
        public string RewardText { get; set; }

        /// <summary>
        /// Gets or sets the achievement reward function.
        /// </summary>
        [JsonProperty(PropertyName = "rewardFunction", Required = Required.Always)]
        public RewardFunction RewardFunction { get; set; }

        /// <summary>
        /// Gets or sets the premium current for this achievement.
        /// </summary>
        [JsonProperty(PropertyName = "premiumCurrency", Required = Required.Always)]
        public int PremiumCurrency { get; set; }

        /// <summary>
        /// Gets or sets the premium current for this achievement.
        /// </summary>
        [JsonProperty(PropertyName = "checkFunction", Required = Required.Always)]
        public CheckFunction CheckFunction { get; set; }

        /// <summary>
        /// Gets or sets the parameters to pass to the <see cref="CheckFunction"/>.
        /// </summary>
        [JsonProperty(PropertyName = "checkParams", Required = Required.Always)]
        [JsonConverter(typeof(StringDoubleListConverter))]
        public double[] CheckFunctionParams { get; set; }

        /// <summary>
        /// Gets or sets the achievement flavor text
        /// </summary>
        [JsonProperty(PropertyName = "flavorText", Required = Required.Always)]
        public string FlavorText { get; set; }

        /// <summary>
        /// Gets or sets the achievement id
        /// </summary>
        [JsonProperty(PropertyName = "id", Required = Required.Always)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the id of the icon to use for this achievement.
        /// </summary>
        [JsonProperty(PropertyName = "iconId", Required = Required.Always)]
        public int IconId { get; set; }

        /// <summary>
        /// Gets or sets the achievement name.
        /// </summary>
        [JsonProperty(PropertyName = "name", Required = Required.Always)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the achievement description.
        /// </summary>
        [JsonProperty(PropertyName = "description", Required = Required.Always)]
        public string Description { get; set; }
    }
}