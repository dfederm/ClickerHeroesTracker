// <copyright file="ItemBonusType.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Game
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents an item (relic) type. Relics possess attributes related to a specific ancient.
    /// </summary>
    [JsonObject]
    public class ItemBonusType
    {
        private static Dictionary<int, int> itemTypeMap = new Dictionary<int, int>
        {
            { 1, AncientIds.Siyalatas },
            { 2, AncientIds.Fragsworth },
            { 3, AncientIds.Chronos },
            { 4, AncientIds.Chawedo },
            { 5, AncientIds.Revolc },
            { 6, AncientIds.Iris },
            { 7, AncientIds.Argaiv },
            { 8, AncientIds.Energon },
            { 9, AncientIds.Kleptos },
            { 10, AncientIds.Sniperino },
            { 11, AncientIds.Berserker },
            { 12, AncientIds.Hecatoncheir },
            { 13, AncientIds.Bubos },
            { 14, AncientIds.Morgulis },
            { 15, AncientIds.Bhaal },
            { 16, AncientIds.Dora },
            { 17, AncientIds.Atman },
            { 18, AncientIds.Fortuna },
            { 19, AncientIds.Dogcog },
            { 20, AncientIds.Pluto },
            { 21, AncientIds.Mimzee },
            { 22, AncientIds.Mammon },
            { 24, AncientIds.Libertas },
            { 25, AncientIds.Solomon },
            { 26, AncientIds.Juggernaut },
            { 27, AncientIds.Kumawakamaru },
            { 28, AncientIds.Vaagur },
        };

        /// <summary>
        /// Gets or sets the scaling for the item bonus
        /// </summary>
        [JsonProperty(PropertyName = "scaling", Required = Required.Always)]
        public Scaling Scaling { get; set; }

        /// <summary>
        /// Gets or sets the max level for the item bonus type.
        /// </summary>
        [JsonProperty(PropertyName = "maxLevel", Required = Required.Always)]
        public int MaxLevel { get; set; }

        /// <summary>
        /// Gets or sets the description for the item bonus.
        /// </summary>
        [JsonProperty(PropertyName = "effectDescription", Required = Required.Always)]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the id for the item bonus type.
        /// </summary>
        [JsonProperty(PropertyName = "id", Required = Required.Always)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name for the item bonus type.
        /// </summary>
        [JsonProperty(PropertyName = "name", Required = Required.Always)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the level amount formula.
        /// </summary>
        [JsonProperty(PropertyName = "levelAmountFormula", Required = Required.Always)]
        public LevelAmountFormula LevelAmountFormula { get; set; }

        /// <summary>
        /// Gets the id of the <see cref="Ancient"/> associated with the item type's attributes.
        /// </summary>
        /// <returns>The id of the ancient that represents the item type, or null if there isn't one.</returns>
        public static int GetAncientId(int itemType)
        {
            int ancientId;
            return itemTypeMap.TryGetValue(itemType, out ancientId)
                ? ancientId
                : 0;
        }
    }
}