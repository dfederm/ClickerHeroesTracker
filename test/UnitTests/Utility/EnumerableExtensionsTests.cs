// <copyright file="EnumerableExtensionsTests.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Tests.Utility
{
    using System;
    using ClickerHeroesTrackerWebsite.Utility;
    using Xunit;

    public class EnumerableExtensionsTests
    {
        [Fact]
        public void EnumerableExtensions_BasicTests()
        {
            AssertChunks("123", 1, "1", "2", "3");
            AssertChunks("111122223333", 4, "1111", "2222", "3333");
            AssertChunks("1111222233", 4, "1111", "2222", "33");
            AssertChunks("11223344556677889900", 2, "11", "22", "33", "44", "55", "66", "77", "88", "99", "00");
        }

        [Fact]
        public void EnumerableExtensions_NullString()
        {
            Assert.Throws<ArgumentNullException>("str", () =>
            {
                string str = null;
                str.SplitIntoChunks(1);
            });
        }

        [Fact]
        public void EnumerableExtensions_ZeroChunks()
        {
            Assert.Throws<ArgumentException>("chunkLength", () =>
            {
                "123".SplitIntoChunks(0);
            });
        }

        [Fact]
        public void EnumerableExtensions_NegativeChunks()
        {
            Assert.Throws<ArgumentException>("chunkLength", () =>
            {
                "123".SplitIntoChunks(-1);
            });
        }

        private static void AssertChunks(string str, int chunkLength, params string[] expectedChunks)
        {
            var actualChunks = str.SplitIntoChunks(chunkLength);
            Assert.Equal(expectedChunks.Length, actualChunks.Length);
            for (var i = 0; i < expectedChunks.Length; i++)
            {
                Assert.Equal(expectedChunks[i], actualChunks[i]);
            }
        }
    }
}
