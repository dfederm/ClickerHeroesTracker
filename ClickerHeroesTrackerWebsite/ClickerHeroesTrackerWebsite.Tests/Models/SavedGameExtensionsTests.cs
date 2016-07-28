// <copyright file="SavedGameExtensionsTests.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Tests.Models
{
    using System.Collections.Generic;
    using ClickerHeroesTrackerWebsite.Models.Game;
    using ClickerHeroesTrackerWebsite.Models.SaveData;
    using Xunit;

    public class SavedGameExtensionsTests
    {
        private static ItemData item1 = new ItemData
        {
            Bonus1Type = 1,
            Bonus1Level = 10,
        };

        private static ItemData item2 = new ItemData
        {
            Bonus1Type = 1,
            Bonus1Level = 10,
            Bonus2Type = 2,
            Bonus2Level = 20,
        };

        private static ItemData item3 = new ItemData
        {
            Bonus1Type = 1,
            Bonus1Level = 10,
            Bonus2Type = 2,
            Bonus2Level = 20,
            Bonus3Type = 3,
            Bonus3Level = 30,
        };

        private static ItemData item4 = new ItemData
        {
            Bonus1Type = 1,
            Bonus1Level = 10,
            Bonus2Type = 2,
            Bonus2Level = 20,
            Bonus3Type = 3,
            Bonus3Level = 30,
            Bonus4Type = 4,
            Bonus4Level = 40,
        };

        private static ItemData item5 = new ItemData
        {
            Bonus1Type = 1,
            Bonus1Level = 99,
            Bonus2Type = 2,
            Bonus2Level = 99,
            Bonus3Type = 3,
            Bonus3Level = 99,
            Bonus4Type = 4,
            Bonus4Level = 99,
        };

        private static Dictionary<int, ItemData> validItems = new Dictionary<int, ItemData>
        {
            { 100, item1 },
            { 200, item2 },
            { 300, item3 },
            { 400, item4 },
            { 500, item5 }, // Extra
        };

        private static Dictionary<int, int> validSlots = new Dictionary<int, int>
        {
            { 1, 100 },
            { 2, 200 },
            { 3, 300 },
            { 4, 400 },
        };

        [Fact]
        public void SavedGameExtensionsTests_GetItemLevels_NullItemsData()
        {
            ItemsData itemsData = null;

            var itemsLevels = itemsData.GetItemLevels();

            Assert.NotNull(itemsLevels);
            Assert.Equal(0, itemsLevels.Count);
        }

        [Fact]
        public void SavedGameExtensionsTests_GetItemLevels_NullSlots()
        {
            ItemsData itemsData = new ItemsData
            {
                Slots = null,
                Items = validItems,
            };

            var itemsLevels = itemsData.GetItemLevels();

            Assert.NotNull(itemsLevels);
            Assert.Equal(0, itemsLevels.Count);
        }

        [Fact]
        public void SavedGameExtensionsTests_GetItemLevels_NullItems()
        {
            ItemsData itemsData = new ItemsData
            {
                Slots = validSlots,
                Items = null,
            };

            var itemsLevels = itemsData.GetItemLevels();

            Assert.NotNull(itemsLevels);
            Assert.Equal(0, itemsLevels.Count);
        }

        [Fact]
        public void SavedGameExtensionsTests_GetItemLevels_EmptyData()
        {
            ItemsData itemsData = new ItemsData
            {
                Slots = new Dictionary<int, int>(),
                Items = new Dictionary<int, ItemData>(),
            };

            var itemsLevels = itemsData.GetItemLevels();

            Assert.NotNull(itemsLevels);
            Assert.Equal(0, itemsLevels.Count);
        }

        [Fact]
        public void SavedGameExtensionsTests_GetItemLevels_UnknownSlots()
        {
            ItemsData itemsData = new ItemsData
            {
                Slots = new Dictionary<int, int>
                {
                    { 0, 100 }, // Slots are 1-based
                    { 91, 100 },
                    { 92, 200 },
                    { 93, 300 },
                    { 94, 400 },
                },
                Items = validItems,
            };

            var itemsLevels = itemsData.GetItemLevels();

            Assert.NotNull(itemsLevels);
            Assert.Equal(0, itemsLevels.Count);
        }

        [Fact]
        public void SavedGameExtensionsTests_GetItemLevels_UnknownItems()
        {
            ItemsData itemsData = new ItemsData
            {
                Slots = validSlots,
                Items = new Dictionary<int, ItemData>
                {
                    { 199, item1 },
                    { 299, item2 },
                    { 399, item3 },
                    { 499, item4 },
                    { 599, item5 },
                },
            };

            var itemsLevels = itemsData.GetItemLevels();

            Assert.NotNull(itemsLevels);
            Assert.Equal(0, itemsLevels.Count);
        }

        [Fact]
        public void SavedGameExtensionsTests_GetItemLevels_Valid()
        {
            ItemsData itemsData = new ItemsData
            {
                Slots = validSlots,
                Items = validItems,
            };

            var itemsLevels = itemsData.GetItemLevels();

            Assert.NotNull(itemsLevels);
            Assert.Equal(4, itemsLevels.Count);
            Assert.Equal(40, itemsLevels[ItemBonusType.GetAncientId(1)]);
            Assert.Equal(60, itemsLevels[ItemBonusType.GetAncientId(2)]);
            Assert.Equal(60, itemsLevels[ItemBonusType.GetAncientId(3)]);
            Assert.Equal(40, itemsLevels[ItemBonusType.GetAncientId(4)]);
        }

        [Fact]
        public void SavedGameExtensionsTests_GetItemLevels_ExtraSlots()
        {
            var extraSlots = new Dictionary<int, int>(validSlots);
            extraSlots.Add(5, extraSlots[1]);
            extraSlots.Add(6, extraSlots[2]);
            extraSlots.Add(7, extraSlots[3]);
            extraSlots.Add(8, extraSlots[4]);

            ItemsData itemsData = new ItemsData
            {
                Slots = extraSlots,
                Items = validItems,
            };

            var itemsLevels = itemsData.GetItemLevels();

            Assert.NotNull(itemsLevels);
            Assert.Equal(4, itemsLevels.Count);
            Assert.Equal(40, itemsLevels[ItemBonusType.GetAncientId(1)]);
            Assert.Equal(60, itemsLevels[ItemBonusType.GetAncientId(2)]);
            Assert.Equal(60, itemsLevels[ItemBonusType.GetAncientId(3)]);
            Assert.Equal(40, itemsLevels[ItemBonusType.GetAncientId(4)]);
        }
    }
}
