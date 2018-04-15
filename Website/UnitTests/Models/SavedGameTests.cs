// <copyright file="SavedGameTests.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Tests.Models
{
    using System.IO;
    using ClickerHeroesTrackerWebsite.Models.SaveData;
    using Xunit;

    public static class SavedGameTests
    {
        [Theory]
        [InlineData("ValidSprinkle", true)]
        [InlineData("InvalidSprinkleMissingAntiCheat", false)]
        [InlineData("InvalidSprinkleBadHash", false)]
        [InlineData("InvalidSprinkleBadData", false)]
        [InlineData("InvalidSprinkleOddLength", false)] // Length of the substring on the left side of the anti-cheat code is a odd number.
        [InlineData("ValidAndroid", true)]
        [InlineData("InvalidAndroidNoBrace", false)]
        [InlineData("InvalidAndroidBadJson", false)]
        [InlineData("ValidZlib", true)]
        [InlineData("InvalidZlibBadData", false)]
        public static void SavedGame_Parse(string testDataName, bool expectedValid)
        {
            var encodedSaveData = File.ReadAllText(Path.Combine("TestData", testDataName + ".txt"));
            var savedGame = SavedGame.Parse(encodedSaveData);

            if (expectedValid)
            {
                Assert.NotNull(savedGame);
            }
            else
            {
                Assert.Null(savedGame);
            }
        }

        // Only need to test valid saves as the Parsing tests handle invalid ones
        [Theory]
        [InlineData("ValidSprinkle")]
        [InlineData("ValidAndroid")]
        [InlineData("ValidZlib")]
        public static void SavedGame_ScrubIdentity(string testDataName)
        {
            var encodedSaveData = File.ReadAllText(Path.Combine("TestData", testDataName + ".txt"));

            var scrubbedSaveData = SavedGame.ScrubIdentity(encodedSaveData);
            Assert.NotNull(scrubbedSaveData);

            // Ensure it can be round-tripped
            var savedGame = SavedGame.Parse(scrubbedSaveData);
            Assert.NotNull(savedGame);
        }
    }
}
