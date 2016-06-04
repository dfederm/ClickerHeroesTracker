// <copyright file="SimulationTests.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Tests.Models
{
    using System.IO;
    using ClickerHeroesTrackerWebsite.Models;
    using ClickerHeroesTrackerWebsite.Models.SaveData;
    using ClickerHeroesTrackerWebsite.Models.Simulation;
    using ClickerHeroesTrackerWebsite.Tests.Mocks;
    using Xunit;

    public class SimulationTests
    {
        [Theory]
        [InlineData("0", 2005, 1922.7, 556452.080, 289.412, PlayStyle.Idle)]
        [InlineData("5374", 140, 57564, 11.932, 0, PlayStyle.Active)]
        [InlineData("6090", 3711, 3600, 0, 0, PlayStyle.Active)]
        [InlineData("17229", 2930, 881.567, 6526900.36, 7403.751, PlayStyle.Idle)]
        [InlineData("18395", 2520, 1681.2, 3064658.265, 1822.899, PlayStyle.Idle)]
        [InlineData("19539", 2305, 1830.333, 1538225.922, 840.408, PlayStyle.Idle)]
        [InlineData("21533", 1125, 3569.233, 49130.303, 13.765, PlayStyle.Idle)]
        [InlineData("27100", 2915, 2568.1, 2342278.588, 912.067, PlayStyle.Active)]
        [InlineData("47220", 1125, 1974.533, 40316.078, 20.418, PlayStyle.Active)]
        private static void Simulation(string testDataName, double level, double time, double souls, double ratio, PlayStyle playStyle)
        {
            var encodedSaveData = File.ReadAllText(Path.Combine("TestData", testDataName + ".txt"));
            var decodedSaveData = SavedGame.DecodeSaveData(encodedSaveData);
            var saveData = SavedGame.DeserializeSavedGame(decodedSaveData);
            var simulationResult = new Simulation(
                MockGameData.RealData,
                saveData,
                playStyle).Run();

            Assert.NotNull(simulationResult);

            const int Precision = 3;
            Assert.Equal(level, simulationResult.Level, Precision);
            Assert.Equal(time, simulationResult.Time, Precision);
            Assert.Equal(souls, simulationResult.Souls, Precision);
            Assert.Equal(ratio, simulationResult.Ratio, Precision);
        }
    }
}
