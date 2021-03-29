// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System.Collections.Generic;
using System.Numerics;
using ClickerHeroesTrackerWebsite.Models.SaveData;
using ClickerHeroesTrackerWebsite.Models.Stats;
using ClickerHeroesTrackerWebsite.Tests.Mocks;
using Xunit;

namespace UnitTests.Models
{
    public static class AncientLevelsModelTests
    {
        [Fact]
        public static void AncientLevelsModel()
        {
            string encodedSaveData = TestData.ReadAllText("ValidZlib.txt");
            SavedGame savedGame = SavedGame.Parse(encodedSaveData);
            AncientLevelsModel ancientLevels = new(MockGameData.RealData, savedGame);

            Dictionary<int, BigInteger> expectedAncientLevels = new()
            {
                { 4, 200 },
                { 5, 200 },
                { 8, 200 },
                { 9, 200 },
                { 10, 0 },
                { 11, 10 },
                { 12, 10 },
                { 13, 10 },
                { 14, 10 },
                { 15, 0 },
                { 16, 40000 },
                { 17, 10 },
                { 18, 10 },
                { 19, 200 },
                { 20, 10 },
                { 21, 10 },
                { 22, 0 },
                { 23, 0 },
                { 24, 0 },
                { 25, 0 },
                { 26, 0 },
                { 27, 0 },
                { 28, 200 },
                { 29, 50 },
                { 31, 10 },
                { 32, 50 },
            };

            Assert.NotNull(ancientLevels.AncientLevels);
            Assert.Equal(expectedAncientLevels.Count, ancientLevels.AncientLevels.Count);
            foreach (KeyValuePair<int, BigInteger> pair in expectedAncientLevels)
            {
                Assert.Equal(pair.Value, ancientLevels.AncientLevels[pair.Key]);
            }
        }
    }
}
