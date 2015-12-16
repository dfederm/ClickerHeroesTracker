// <copyright file="Extensions.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.SaveData
{
    using System;
    using System.Collections.Generic;
    using ClickerHeroesTrackerWebsite.Models.Game;

    internal static class Extensions
    {
        public static long GetAncientLevel(this AncientsData ancientsData, int ancientId)
        {
            AncientData ancientData;
            return ancientsData.Ancients.TryGetValue(ancientId, out ancientData)
                ? ancientData.Level
                : 0;
        }

        public static int GetHeroGilds(this HeroesData heroesData, Hero hero)
        {
            HeroData heroData;
            return heroesData != null && heroesData.Heroes.TryGetValue(hero.Id, out heroData)
                ? heroData.Gilds
                : 0;
        }

        public static int GetItemLevel(this IDictionary<int, int> itemLevels, int ancientId)
        {
            int itemLevel;
            return itemLevels.TryGetValue(ancientId, out itemLevel)
                ? itemLevel
                : 0;
        }

        public static IDictionary<int, int> GetItemLevels(this ItemsData itemsData)
        {
            var itemLevels = new Dictionary<int, int>();

            if (itemsData != null)
            {
                var numActiveItems = Math.Min(4, itemsData.Slots.Count);
                for (var i = 0; i < numActiveItems; i++)
                {
                    int itemId;
                    if (!itemsData.Slots.TryGetValue(i + 1, out itemId))
                    {
                        continue;
                    }

                    ItemData itemData;
                    if (!itemsData.Items.TryGetValue(itemId, out itemData))
                    {
                        continue;
                    }

                    AddItemLevels(itemLevels, itemData.Bonus1Type, itemData.Bonus1Level);
                    AddItemLevels(itemLevels, itemData.Bonus2Type, itemData.Bonus2Level);
                    AddItemLevels(itemLevels, itemData.Bonus3Type, itemData.Bonus3Level);
                    AddItemLevels(itemLevels, itemData.Bonus4Type, itemData.Bonus4Level);
                }
            }

            return itemLevels;
        }

        private static void AddItemLevels(
            Dictionary<int, int> itemLevels,
            int? itemType,
            int? itemLevel)
        {
            if (itemType == null
                || itemLevel == null
                || itemLevel.Value == 0)
            {
                return;
            }

            var ancientId = ItemBonusType.GetAncientId(itemType.Value);
            if (ancientId == 0)
            {
                return;
            }

            int currentLevel;
            if (itemLevels.TryGetValue(ancientId, out currentLevel))
            {
                itemLevels[ancientId] = currentLevel + itemLevel.Value;
            }
            else
            {
                itemLevels.Add(ancientId, itemLevel.Value);
            }
        }
    }
}