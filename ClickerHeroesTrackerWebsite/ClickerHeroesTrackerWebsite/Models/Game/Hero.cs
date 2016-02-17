// <copyright file="Hero.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Game
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;
    using Utility;

    /// <summary>
    /// Represents a hero in the game
    /// </summary>
    [JsonObject]
    public class Hero
    {
        /// <summary>
        /// Gets or sets the base click damage for the hero.
        /// </summary>
        [JsonProperty(PropertyName = "baseClickDamage", Required = Required.Always)]
        public double BaseClickDamage { get; set; }

        /// <summary>
        /// Gets or sets the base gold per second for the hero.
        /// </summary>
        /// <remarks>
        /// This has no known use.
        /// </remarks>
        [JsonProperty(PropertyName = "baseGoldPerSecond", Required = Required.Always)]
        public double BaseGoldPerSecond { get; set; }

        /// <summary>
        /// Gets or sets the base gold cost for the hero.
        /// </summary>
        [JsonProperty(PropertyName = "baseCost", Required = Required.Always)]
        public double BaseCost { get; set; }

        /// <summary>
        /// Gets or sets the base gold cost for the hero.
        /// </summary>
        [JsonProperty(PropertyName = "attackFormula", Required = Required.Always)]
        public HeroAttackFormula AttackFormula { get; set; }

        /// <summary>
        /// Gets or sets the id of the hero
        /// </summary>
        [JsonProperty(PropertyName = "id", Required = Required.Always)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the id of the hero
        /// </summary>
        [JsonProperty(PropertyName = "clickDamageFormula", Required = Required.Always)]
        public HeroClickDamageFormula ClickDamageFormula { get; set; }

        /// <summary>
        /// Gets or sets the name of the hero
        /// </summary>
        [JsonProperty(PropertyName = "name", Required = Required.Always)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the gold per second formula
        /// </summary>
        [JsonProperty(PropertyName = "goldPerSecondFormula", Required = Required.Always)]
        public HeroGoldPerSecondFormula GoldPerSecondFormula { get; set; }

        /// <summary>
        /// Gets or sets the hero's cost formula
        /// </summary>
        [JsonProperty(PropertyName = "costFormula", Required = Required.Always)]
        public HeroCostFormula CostFormula { get; set; }

        /// <summary>
        /// Gets or sets the hero's special skill.
        /// </summary>
        /// <remarks>
        /// This has no known use.
        /// </remarks>
        [JsonProperty(PropertyName = "specialSkill", Required = Required.Always)]
        public string SpecialSkill { get; set; }

        /// <summary>
        /// Gets or sets the description of the hero's special skill.
        /// </summary>
        /// <remarks>
        /// This has no known use.
        /// </remarks>
        [JsonProperty(PropertyName = "specialSkillDescription", Required = Required.Always)]
        public string SpecialSkillDescription { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this hero is live (enabled).
        /// </summary>
        [JsonProperty(PropertyName = "_live", Required = Required.Always)]
        [JsonConverter(typeof(ZeroOneBoolConverter))]
        public bool Live { get; set; }

        /// <summary>
        /// Gets or sets the asset id to use for this hero.
        /// </summary>
        [JsonProperty(PropertyName = "assetId", Required = Required.Always)]
        public int AssetId { get; set; }

        /// <summary>
        /// Gets or sets the hero's base attack
        /// </summary>
        [JsonProperty(PropertyName = "baseAttack", Required = Required.Always)]
        public double BaseAttack { get; set; }

        /// <summary>
        /// Gets or sets the hero's description
        /// </summary>
        [JsonProperty(PropertyName = "description", Required = Required.Always)]
        public string Description { get; set; }

        /// <summary>
        /// Gets a value indicating whether the hero is a ranger.
        /// </summary>
        [JsonIgnore]
        public bool IsRanger
        {
            get
            {
                return this.Id >= 27;
            }
        }
    }
}