// <copyright file="DateTimeExtensionTests.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Tests.Utility
{
    using ClickerHeroesTrackerWebsite.Utility;
    using Xunit;

    public class DateTimeExtensionTests
    {
        [Theory]
        [InlineData(1469496451, "2016-07-26T01:27:31")]
        [InlineData(1469922597, "2016-07-30T23:49:57")]
        [InlineData(946684801, "2000-01-01T00:00:01")]
        private static void UnixTimeStampToDateTimeTest(double unixTimestamp, string expectedDatetime)
        {
            Assert.Equal(expectedDatetime, unixTimestamp.UnixTimeStampToDateTime().ToString("s"));
        }
    }
}
