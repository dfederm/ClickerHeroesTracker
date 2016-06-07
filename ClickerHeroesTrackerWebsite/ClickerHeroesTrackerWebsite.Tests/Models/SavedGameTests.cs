// <copyright file="SavedGameTests.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Tests.Models
{
    using System.IO;
    using ClickerHeroesTrackerWebsite.Models.SaveData;
    using Xunit;

    public class SavedGameTests
    {
        [Theory]
        [InlineData("ValidEncoded", true)]
        [InlineData("InvalidEncodedMissingAntiCheat", false)]
        [InlineData("InvalidEncodedBadHash", false)]
        [InlineData("InvalidEncodedBadData", false)]
        [InlineData("ValidEncodedAndroid", true)]
        [InlineData("InvalidEncodedAndroidNoBrace", false)]
        [InlineData("InvalidEncodedOddLength", false)] // Length of the substring on the left side of the anti-cheat code is a odd number.
        public void SavedGame_DecodeSaveData(string testDataName, bool expectedValid)
        {
            var encodedSaveData = File.ReadAllText(Path.Combine("TestData", testDataName + ".txt"));
            var reader = SavedGame.DecodeSaveData(encodedSaveData);

            if (expectedValid)
            {
                Assert.NotNull(reader);
                Assert.NotEqual(-1, reader.Peek());
            }
            else
            {
                Assert.Null(reader);
            }
        }

        [Theory]
        [InlineData("InvalidEncodedAndroidBadJson")]
        public void SavedGame_DecodeAndDeserializeSaveData(string testDataName)
        {
            var encodedSaveData = File.ReadAllText(Path.Combine("TestData", testDataName + ".txt"));
            var reader = SavedGame.DecodeSaveData(encodedSaveData);
            var savedGame = SavedGame.DeserializeSavedGame(reader);

            Assert.Null(savedGame);
        }

        [Theory]
        [InlineData("ValidDecoded-0.19", true)]
        [InlineData("ValidDecoded-0.99-beta", true)]
        [InlineData("InvalidDecodedNotJson", false)]
        [InlineData("InvalidDecodedEmptyObject", true)]
        public void SavedGame_DeserializeSavedGame(string testDataName, bool expectedValid)
        {
            var reader = File.OpenText(Path.Combine("TestData", testDataName + ".txt"));
            var savedGame = SavedGame.DeserializeSavedGame(reader);

            if (expectedValid)
            {
                Assert.NotNull(savedGame);
            }
            else
            {
                Assert.Null(savedGame);
            }
        }

        [Theory]
        [InlineData("ValidEncoded", false)]
        [InlineData("InvalidEncodedBadData", false)]
        [InlineData("InvalidEncodedBadHash", false)]
        [InlineData("InvalidEncodedMissingAntiCheat", false)]
        [InlineData("ValidEncodedAndroid", true)]
        [InlineData("InvalidEncodedAndroidNoBrace", true)]
        [InlineData("InvalidEncodedAndroidBadJson", true)]
        public void SavedGame_IsAndroid(string testDataName, bool expectedIsAndroid)
        {
            var encodedSaveData = File.ReadAllText(Path.Combine("TestData", testDataName + ".txt"));

            Assert.Equal(expectedIsAndroid, SavedGame.IsAndroid(encodedSaveData));
        }
    }
}
