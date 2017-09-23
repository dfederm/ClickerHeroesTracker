// <copyright file="BigIntegerExtensionsTests.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace UnitTests.Utility
{
    using ClickerHeroesTrackerWebsite.Utility;
    using Xunit;

    public sealed class BigIntegerExtensionsTests
    {
        // Tests both ToBigInteger and ToTransportableString
        [Theory]
        [InlineData("0", "0.000000000000000000e+000")]
        [InlineData("1", "1.000000000000000000e+000")]
        [InlineData("2", "2.000000000000000000e+000")]
        [InlineData("1.99999e2", "1.990000000000000000e+002")]
        [InlineData("1e10", "1.000000000000000000e+010")]
        [InlineData("1.23456789e100", "1.234567890000000000e+100")]
        [InlineData("1.23456789e1234", "1.234567890000000000e+1234")]
        public void RoundTrip(string str, string expected)
        {
            var bigInt = str.ToBigInteger();
            var transportableString = bigInt.ToTransportableString();

            Assert.Equal(expected, transportableString);
            Assert.Equal(bigInt, transportableString.ToBigInteger());
        }
    }
}
