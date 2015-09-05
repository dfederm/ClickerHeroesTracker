namespace ClickerHeroesTrackerWebsite.Models.SaveData
{
    using ClickerHeroesTrackerWebsite.Models.Game;
    using System;
    using System.Collections.Generic;

    public static class Extensions
    {
        public static long GetAncientLevel(this AncientsData ancientsData, Ancient ancient)
        {
            AncientData ancientData;
            return ancientsData.Ancients.TryGetValue(ancient.Id, out ancientData)
                ? ancientData.Level
                : 0;
        }

        public static int GetHeroLevel(this HeroesData heroesData, Hero hero)
        {
            HeroData heroData;
            return heroesData.Heroes.TryGetValue(hero.Id, out heroData)
                ? heroData.Level
                : 0;
        }

        public static int GetHeroGilds(this HeroesData heroesData, Hero hero)
        {
            HeroData heroData;
            return heroesData != null && heroesData.Heroes.TryGetValue(hero.Id, out heroData)
                ? heroData.Gilds
                : 0;
        }

        public static int GetItemLevel(this IDictionary<Ancient, int> itemLevels, Ancient ancient)
        {
            int itemLevel;
            return itemLevels.TryGetValue(ancient, out itemLevel)
                ? itemLevel
                : 0;
        }

        public static IDictionary<Ancient, int> GetItemLevels(this ItemsData itemsData)
        {
            var itemLevels = new Dictionary<Ancient, int>();

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

        private static void AddItemLevels(Dictionary<Ancient, int> itemLevels, int? itemType, int? itemLevel)
        {
            if (itemType == null
                || itemLevel == null
                || itemLevel.Value == 0)
            {
                return;
            }

            var ancient = ItemTypes.GetAncient(itemType.Value);
            if (ancient == null)
            {
                return;
            }

            int currentLevel;
            if (itemLevels.TryGetValue(ancient, out currentLevel))
            {
                itemLevels[ancient] = currentLevel + itemLevel.Value;
            }
            else
            {
                itemLevels.Add(ancient, itemLevel.Value);
            }
        }
    }
}