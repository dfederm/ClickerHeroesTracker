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
        public static double GetAncientLevel(this AncientsData ancientsData, int ancientId)
        {
            AncientData ancientData;
            return ancientsData.Ancients.TryGetValue(ancientId, out ancientData)
                ? ancientData.Level
                : 0;
        }

        public static double GetItemLevel(this IDictionary<int, double> itemLevels, int ancientId)
        {
            double itemLevel;
            return itemLevels.TryGetValue(ancientId, out itemLevel)
                ? itemLevel
                : 0;
        }

        public static double GetOutsiderLevel(this IDictionary<int, OutsiderData> outsiderLevels, int outsiderId)
        {
            OutsiderData outsiderData;
            return outsiderLevels.TryGetValue(outsiderId, out outsiderData)
                ? outsiderData.Level
                : 0;
        }

        public static IDictionary<int, double> GetItemLevels(this ItemsData itemsData, GameData gameData)
        {
            var itemLevels = new Dictionary<int, double>();

            if (itemsData != null
                && itemsData.Slots != null
                && itemsData.Items != null)
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

                    AddItemLevels(gameData, itemLevels, itemData.Bonus1Type, itemData.Bonus1Level);
                    AddItemLevels(gameData, itemLevels, itemData.Bonus2Type, itemData.Bonus2Level);
                    AddItemLevels(gameData, itemLevels, itemData.Bonus3Type, itemData.Bonus3Level);
                    AddItemLevels(gameData, itemLevels, itemData.Bonus4Type, itemData.Bonus4Level);
                }
            }

            return itemLevels;
        }

        private static void AddItemLevels(
            GameData gameData,
            Dictionary<int, double> itemLevels,
            int? itemType,
            double? itemLevel)
        {
            if (itemType == null
                || itemLevel == null
                || itemLevel.Value == 0)
            {
                return;
            }

            ItemBonusType itemBonusType;
            if (!gameData.ItemBonusTypes.TryGetValue(itemType.Value, out itemBonusType))
            {
                return;
            }

            var ancientId = itemBonusType.AncientId;
            double currentLevel;
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