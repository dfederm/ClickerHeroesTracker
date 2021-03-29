// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using ClickerHeroesTrackerWebsite.Utility;
using Xunit;

namespace UnitTests.Utility
{
    public static class BigIntegerExtensionsTests
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
        public static void RoundTrip(string str, string expected)
        {
            System.Numerics.BigInteger bigInt = str.ToBigInteger();
            string transportableString = bigInt.ToTransportableString();

            Assert.Equal(expected, transportableString);
            Assert.Equal(bigInt, transportableString.ToBigInteger());
        }
    }
}
