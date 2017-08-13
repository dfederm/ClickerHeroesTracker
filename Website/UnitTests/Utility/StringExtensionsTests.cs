// <copyright file="StringExtensionsTests.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Tests.Utility
{
    using System;
    using ClickerHeroesTrackerWebsite.Utility;
    using Xunit;

    public class StringExtensionsTests
    {
        [Theory]
        [InlineData("123", 1, "1", "2", "3")]
        [InlineData("111122223333", 4, "1111", "2222", "3333")]
        [InlineData("1111222233", 4, "1111", "2222", "33")]
        [InlineData("11223344556677889900", 2, "11", "22", "33", "44", "55", "66", "77", "88", "99", "00")]
        public void StringExtensions_BasicTests(string str, int chunkLength, params string[] expectedChunks)
        {
            var actualChunks = str.SplitIntoChunks(chunkLength);
            Assert.Equal(expectedChunks.Length, actualChunks.Length);
            for (var i = 0; i < expectedChunks.Length; i++)
            {
                Assert.Equal(expectedChunks[i], actualChunks[i]);
            }
        }

        [Fact]
        public void StringExtensions_NullString()
        {
            Assert.Throws<ArgumentNullException>("str", () =>
            {
                string str = null;
                str.SplitIntoChunks(1);
            });
        }

        [Theory]
        [InlineData("123", 0)]
        [InlineData("123", -1)]
        public void StringExtensions_ChunkLengthExceptions(string str, int chunkLength)
        {
            Assert.Throws<ArgumentException>("chunkLength", () =>
            {
                str.SplitIntoChunks(chunkLength);
            });
        }
    }
}
