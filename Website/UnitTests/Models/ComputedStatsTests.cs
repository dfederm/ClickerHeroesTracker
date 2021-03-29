// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using ClickerHeroesTrackerWebsite.Models.SaveData;
using ClickerHeroesTrackerWebsite.Models.Stats;
using Xunit;

namespace UnitTests.Models
{
    public static class ComputedStatsTests
    {
        [Fact]
        public static void ComputedStats()
        {
            string encodedSaveData = TestData.ReadAllText("ValidZlib.txt");
            SavedGame savedGame = SavedGame.Parse(encodedSaveData);
            ComputedStats computedStats = new(savedGame);

            Assert.Equal("1.002390000000000000e+005", computedStats.HeroSoulsSpent);
            Assert.Equal("5.223865765430567e99", computedStats.HeroSoulsSacrificed);
            Assert.Equal("5.22386475134114e99", computedStats.TitanDamage);
            Assert.Equal(498, computedStats.TotalAncientSouls);
            Assert.Equal(0.051918352081052083, computedStats.TranscendentPower);
            Assert.Equal(260, computedStats.Rubies);
            Assert.Equal(695, computedStats.HighestZoneThisTranscension);
            Assert.Equal(23274, computedStats.HighestZoneLifetime);
            Assert.Equal(4, computedStats.AscensionsThisTranscension);
            Assert.Equal(3080, computedStats.AscensionsLifetime);
        }
    }
}
