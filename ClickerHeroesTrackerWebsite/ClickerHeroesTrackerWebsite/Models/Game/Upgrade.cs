// <copyright file="Upgrade.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Game
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Utility;

    /// <summary>
    /// Represents an upgrade for a hero in the game.
    /// </summary>
    [JsonObject]
    public class Upgrade
    {
        private static readonly Dictionary<int, double> AttributeMultiplier = new Dictionary<int, double>()
        {
            { 1, 10 },
            { 2, 25 },
            { 3, 100 },
            { 4, 800 },
            { 5, 8000 },
            { 6, 40000 },
            { 7, 400000 },
        };

        /// <summary>
        /// Gets or sets the hero id that this upgrade belongs to
        /// </summary>
        [JsonProperty(PropertyName = "heroId", Required = Required.Always)]
        public int HeroId { get; set; }

        /// <summary>
        /// Gets or sets the upgrade's upgrade function
        /// </summary>
        [JsonProperty(PropertyName = "upgradeFunction", Required = Required.Always)]
        public UpgradeFunction UpgradeFunction { get; set; }

        /// <summary>
        /// Gets or sets the attribute, which determines how cost scaling works for this upgrade.
        /// </summary>
        [JsonProperty(PropertyName = "attribute", Required = Required.Always)]
        public int Attribute { get; set; }

        /// <summary>
        /// Gets or sets the parameters to pass to the <see cref="UpgradeFunction"/>.
        /// </summary>
        [JsonProperty(PropertyName = "upgradeParams", Required = Required.Always)]
        [JsonConverter(typeof(StringDoubleListConverter))]
        public double[] UpgradeFunctionParameters { get; set; }

        /// <summary>
        /// Gets or sets the upgrade name
        /// </summary>
        [JsonProperty(PropertyName = "name", Required = Required.Always)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the upgrade is a percentage upgrade.
        /// </summary>
        [JsonProperty(PropertyName = "isPercentage", Required = Required.Always)]
        [JsonConverter(typeof(ZeroOneBoolConverter))]
        public bool IsPercentage { get; set; }

        /// <summary>
        /// Gets or sets the id of the prerequisite upgrade, if any. 0 indicates it has none.
        /// </summary>
        [JsonProperty(PropertyName = "upgradeRequired", Required = Required.Always)]
        public int UpgradeRequired { get; set; }

        /// <summary>
        /// Gets or sets the upgrade id
        /// </summary>
        [JsonProperty(PropertyName = "id", Required = Required.Always)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the required hero level to buy the upgrade
        /// </summary>
        [JsonProperty(PropertyName = "heroLevelRequired", Required = Required.Always)]
        public int HeroLevelRequired { get; set; }

        /// <summary>
        /// Gets or sets the amount
        /// </summary>
        /// <remarks>
        /// This has no known use.
        /// </remarks>
        [JsonProperty(PropertyName = "amount", Required = Required.Always)]
        public int Amount { get; set; }

        /// <summary>
        /// Gets or sets the display order for the upgrade.
        /// </summary>
        [JsonProperty(PropertyName = "displayOrder", Required = Required.Always)]
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this upgrade is live (enabled).
        /// </summary>
        [JsonProperty(PropertyName = "_live", Required = Required.Always)]
        [JsonConverter(typeof(ZeroOneBoolConverter))]
        public bool Live { get; set; }

        /// <summary>
        /// Gets or sets the id of the icon to use for the upgrade.
        /// </summary>
        [JsonProperty(PropertyName = "iconId", Required = Required.Always)]
        public int IconId { get; set; }

        /// <summary>
        /// Gets or sets the upgrade description.
        /// </summary>
        [JsonProperty(PropertyName = "description", Required = Required.Always)]
        public string Description { get; set; }

        /// <summary>
        /// Gets the upgrade cost
        /// </summary>
        /// <returns>The upgrade cost.</returns>
        public double GetCost(Hero hero)
        {
            if (hero.Id != this.HeroId)
            {
                throw new InvalidOperationException($"The hero id {hero.Id} did not match the upgrade's hero id of {this.HeroId}");
            }

            double attributeMultiplier;
            if (!AttributeMultiplier.TryGetValue(this.Attribute, out attributeMultiplier))
            {
                throw new InvalidOperationException($"The attribute id {this.Attribute} was not valid");
            }

            return hero.BaseCost * attributeMultiplier;
        }
    }
}