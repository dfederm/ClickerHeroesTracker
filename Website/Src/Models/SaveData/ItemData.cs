// <copyright file="ItemData.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.SaveData
{
    using System.Numerics;
    using ClickerHeroesTrackerWebsite.Utility;
    using Newtonsoft.Json;

    /// <summary>
    /// Save game data for an item.
    /// </summary>
    [JsonObject]
    public class ItemData
    {
        /// <summary>
        /// Gets or sets the first item bonus type.
        /// </summary>
        [JsonProperty(PropertyName = "bonusType1")]
        public int? Bonus1Type { get; set; }

        /// <summary>
        /// Gets or sets the first item bonus level.
        /// </summary>
        [JsonProperty(PropertyName = "bonus1Level")]
        [JsonConverter(typeof(BigIntegerStringConverter))]
        public BigInteger? Bonus1Level { get; set; }

        /// <summary>
        /// Gets or sets the second item bonus type.
        /// </summary>
        [JsonProperty(PropertyName = "bonusType2")]
        public int? Bonus2Type { get; set; }

        /// <summary>
        /// Gets or sets the second item bonus level.
        /// </summary>
        [JsonProperty(PropertyName = "bonus2Level")]
        [JsonConverter(typeof(BigIntegerStringConverter))]
        public BigInteger? Bonus2Level { get; set; }

        /// <summary>
        /// Gets or sets the third item bonus type.
        /// </summary>
        [JsonProperty(PropertyName = "bonusType3")]
        public int? Bonus3Type { get; set; }

        /// <summary>
        /// Gets or sets the third item bonus level.
        /// </summary>
        [JsonProperty(PropertyName = "bonus3Level")]
        [JsonConverter(typeof(BigIntegerStringConverter))]
        public BigInteger? Bonus3Level { get; set; }

        /// <summary>
        /// Gets or sets the fourth item bonus type.
        /// </summary>
        [JsonProperty(PropertyName = "bonusType4")]
        public int? Bonus4Type { get; set; }

        /// <summary>
        /// Gets or sets the fourth item bonus level.
        /// </summary>
        [JsonProperty(PropertyName = "bonus4Level")]
        [JsonConverter(typeof(BigIntegerStringConverter))]
        public BigInteger? Bonus4Level { get; set; }
    }
}