// <copyright file="OutsiderLevelsModelTests.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace UnitTests.Models
{
    using System.Collections.Generic;
    using System.IO;
    using ClickerHeroesTrackerWebsite.Models.SaveData;
    using ClickerHeroesTrackerWebsite.Models.Stats;
    using ClickerHeroesTrackerWebsite.Tests.Mocks;
    using Xunit;

    public class OutsiderLevelsModelTests
    {
        [Fact]
        public void OutsiderLevelsModel()
        {
            var encodedSaveData = File.ReadAllText("TestData\\ValidZlib.txt");
            var savedGame = SavedGame.Parse(encodedSaveData);
            var outsiderLevels = new OutsiderLevelsModel(MockGameData.RealData, savedGame);

            var expectedOutsiderLevels = new Dictionary<int, long>
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
            foreach (var pair in expectedOutsiderLevels)
            {
                Assert.Equal(pair.Value, outsiderLevels.OutsiderLevels[pair.Key]);
            }
        }
    }
}
