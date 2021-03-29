// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System.Collections.Generic;
using ClickerHeroesTrackerWebsite.Models.SaveData;
using ClickerHeroesTrackerWebsite.Models.Stats;
using ClickerHeroesTrackerWebsite.Tests.Mocks;
using Xunit;

namespace UnitTests.Models
{
    public static class OutsiderLevelsModelTests
    {
        [Fact]
        public static void OutsiderLevelsModel()
        {
            string encodedSaveData = TestData.ReadAllText("ValidZlib.txt");
            SavedGame savedGame = SavedGame.Parse(encodedSaveData);
            OutsiderLevelsModel outsiderLevels = new(MockGameData.RealData, savedGame);

            Dictionary<int, long> expectedOutsiderLevels = new()
            {
                { 1, 0 },
                { 2, 0 },
                { 3, 0 },
                { 5, 0 },
                { 6, 0 },
                { 7, 0 },
                { 8, 0 },
                { 9, 0 },
                { 10, 0 },
            };

            Assert.NotNull(outsiderLevels.OutsiderLevels);
            Assert.Equal(expectedOutsiderLevels.Count, outsiderLevels.OutsiderLevels.Count);
            foreach (KeyValuePair<int, long> pair in expectedOutsiderLevels)
            {
                Assert.Equal(pair.Value, outsiderLevels.OutsiderLevels[pair.Key]);
            }
        }
    }
}
