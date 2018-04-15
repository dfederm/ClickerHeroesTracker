// <copyright file="AncientLevelsModelTests.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace UnitTests.Models
{
    using System.Collections.Generic;
    using System.IO;
    using System.Numerics;
    using ClickerHeroesTrackerWebsite.Models.SaveData;
    using ClickerHeroesTrackerWebsite.Models.Stats;
    using ClickerHeroesTrackerWebsite.Tests.Mocks;
    using Xunit;

    public static class AncientLevelsModelTests
    {
        [Fact]
        public static void AncientLevelsModel()
        {
            var encodedSaveData = File.ReadAllText("TestData\\ValidZlib.txt");
            var savedGame = SavedGame.Parse(encodedSaveData);
            var ancientLevels = new AncientLevelsModel(MockGameData.RealData, savedGame);

            var expectedAncientLevels = new Dictionary<int, BigInteger>
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
            foreach (var pair in expectedAncientLevels)
            {
                Assert.Equal(pair.Value, ancientLevels.AncientLevels[pair.Key]);
            }
        }
    }
}
