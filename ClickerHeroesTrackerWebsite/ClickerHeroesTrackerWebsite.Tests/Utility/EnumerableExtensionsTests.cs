// <copyright file="EnumerableExtensionsTests.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Tests.Utility
{
    using System;
    using ClickerHeroesTrackerWebsite.Utility;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class EnumerableExtensionsTests
    {
        [TestMethod]
        public void EnumerableExtensions_BasicTests()
        {
            AssertChunks("123", 1, "1", "2", "3");
            AssertChunks("111122223333", 4, "1111", "2222", "3333");
            AssertChunks("1111222233", 4, "1111", "2222", "33");
            AssertChunks("11223344556677889900", 2, "11", "22", "33", "44", "55", "66", "77", "88", "99", "00");
        }

        [TestMethod]
        public void EnumerableExtensions_NullString()
        {
            try
            {
                string str = null;
                str.SplitIntoChunks(1);

                Assert.Fail();
            }
            catch (ArgumentNullException e)
            {
                Assert.AreEqual("str", e.ParamName);
            }
        }

        [TestMethod]
        public void EnumerableExtensions_ZeroChunks()
        {
            try
            {
                "123".SplitIntoChunks(0);
                Assert.Fail();
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual("chunkLength", e.ParamName);
            }
        }

        [TestMethod]
        public void EnumerableExtensions_NegativeChunks()
        {
            try
            {
                "123".SplitIntoChunks(-1);
                Assert.Fail();
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual("chunkLength", e.ParamName);
            }
        }

        private static void AssertChunks(string str, int chunkLength, params string[] expectedChunks)
        {
            var actualChunks = str.SplitIntoChunks(chunkLength);
            Assert.AreEqual(expectedChunks.Length, actualChunks.Length);
            for (var i = 0; i < expectedChunks.Length; i++)
            {
                Assert.AreEqual(expectedChunks[i], actualChunks[i]);
            }
        }
    }
}
