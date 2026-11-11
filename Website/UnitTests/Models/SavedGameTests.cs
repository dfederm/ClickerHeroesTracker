// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using ClickerHeroesTrackerWebsite.Models.SaveData;
using Xunit;

namespace UnitTests.Models
{
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
        [InlineData("ValidDeflate", true)]
        [InlineData("InvalidZlibBadData", false)]
        public static void SavedGame_Parse(string testDataName, bool expectedValid)
        {
            string encodedSaveData = TestData.ReadAllText(testDataName + ".txt");
            SavedGame savedGame = SavedGame.Parse(encodedSaveData);

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
        [InlineData("ValidDeflate")]
        public static void SavedGame_ScrubIdentity(string testDataName)
        {
            string encodedSaveData = File.ReadAllText(Path.Combine("TestData", testDataName + ".txt"));

            string scrubbedSaveData = SavedGame.ScrubIdentity(encodedSaveData);
            Assert.NotNull(scrubbedSaveData);

            // Ensure it can be round-tripped
            SavedGame savedGame = SavedGame.Parse(scrubbedSaveData);
            Assert.NotNull(savedGame);
        }

        [Theory]
        [InlineData("@@==")]
        [InlineData("A===")]
        [InlineData("AB==AAAA")]
        public static void Parse_RejectsMalformedBase64(string data)
        {
            Assert.Null(SavedGame.Parse("7e8bb5a89f2842ac4af01b3b7e228592" + data));
            Assert.Null(SavedGame.Parse(CreateSprinkleSave(data)));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public static void ScrubIdentity_PreservesUnicodeAndLongText(bool zlib)
        {
            string save = CreateCompressedSave(zlib);
            SavedGame original = SavedGame.Parse(save);
            SavedGame scrubbed = SavedGame.Parse(SavedGame.ScrubIdentity(save));

            Assert.NotNull(original);
            Assert.NotNull(scrubbed);
            Assert.Equal(original.Object.Value<string>("description"), scrubbed.Object.Value<string>("description"));
            Assert.Contains("\u00e9\ud83d\ude80", scrubbed.Object.Value<string>("description"), StringComparison.Ordinal);
            Assert.Equal(string.Empty, scrubbed.Object.Value<string>("email"));
        }

        private static string CreateCompressedSave(bool zlib)
        {
            string prefix = zlib ? "7a990d405d2c6fb93aa8fbb0ec1a3b23" : "7e8bb5a89f2842ac4af01b3b7e228592";
            string json = "{\"email\":\"test\",\"description\":\"" + new string('a', 20000) + "\\u00e9\\ud83d\\ude80\"}";
            using MemoryStream output = new();
            using (Stream compression = zlib
                ? new ZLibStream(output, CompressionMode.Compress, leaveOpen: true)
                : new DeflateStream(output, CompressionMode.Compress, leaveOpen: true))
            {
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                compression.Write(bytes);
            }

            return prefix + Convert.ToBase64String(output.ToArray());
        }

        private static string CreateSprinkleSave(string data)
        {
            StringBuilder result = new();
            foreach (char c in data)
            {
                result.Append(c);
                result.Append('0');
            }

            byte[] hash = MD5.HashData(Encoding.UTF8.GetBytes(data + "af0ik392jrmt0nsfdghy0"));
            return result + "Fe12NAfA3R6z4k0z" + Convert.ToHexStringLower(hash);
        }
    }
}
