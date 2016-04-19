// <copyright file="BigDecimalTests.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Tests.Utility
{
    using System;
    using System.Numerics;
    using System.Text;
    using ClickerHeroesTrackerWebsite.Utility;
    using Xunit;

    public class BigDecimalTests
    {
        [Fact]
        public void BigDecimal_ToString_Zero()
        {
            var number = new BigDecimal(BigInteger.Zero, 0);
            Assert.Equal("0", number.ToString());
            Assert.Equal("0", number.ToString((string)null));
            Assert.Equal("0", number.ToString((IFormatProvider)null));
            Assert.Equal("0", number.ToString(null, null));
            Assert.Equal("0", number.ToString("G"));
            Assert.Equal("0.000000E+000", number.ToString("E"));
            Assert.Equal("0.0000E+000", number.ToString("E4"));
        }

        [Fact]
        public void BigDecimal_ToString_One()
        {
            var number = new BigDecimal(BigInteger.One, 0);
            AssertNullConsistency(number);
            Assert.Equal("1", number.ToString());
            Assert.Equal("1", number.ToString("G"));
            Assert.Equal("1.000000E+000", number.ToString("E"));
            Assert.Equal("1.0000E+000", number.ToString("E4"));
        }

        [Fact]
        public void BigDecimal_ToString_MinusOne()
        {
            var number = new BigDecimal(BigInteger.MinusOne, 0);
            AssertNullConsistency(number);
            Assert.Equal("-1", number.ToString());
            Assert.Equal("-1", number.ToString("G"));
            Assert.Equal("-1.000000E+000", number.ToString("E"));
            Assert.Equal("-1.0000E+000", number.ToString("E4"));
        }

        [Fact]
        public void BigDecimal_ToString_PositiveExponent()
        {
            var number = new BigDecimal(2, 2);
            AssertNullConsistency(number);
            Assert.Equal("200", number.ToString());
            Assert.Equal("200", number.ToString("G"));
            Assert.Equal("2.000000E+002", number.ToString("E"));
            Assert.Equal("2.0000E+002", number.ToString("E4"));
        }

        [Fact]
        public void BigDecimal_ToString_NegativeExponent()
        {
            var number = new BigDecimal(2, -2);
            AssertNullConsistency(number);
            Assert.Equal("0.02", number.ToString());
            Assert.Equal("0.02", number.ToString("G"));
            Assert.Equal("2.000000E-002", number.ToString("E"));
            Assert.Equal("2.0000E-002", number.ToString("E4"));
        }

        [Fact]
        public void BigDecimal_ToString_HighPrecisionDoubleRangeNumber()
        {
            const int NumDigits = 100;
            const int Digit = 5;
            var digitString = Digit.ToString();
            var number = new BigDecimal(BigInteger.Zero, 0);
            var expectedStringBuilder = new StringBuilder(NumDigits);
            for (int i = 0; i < NumDigits; i++)
            {
                number *= 10;
                number += Digit;
                expectedStringBuilder.Append(digitString);
            }

            var expectedString = expectedStringBuilder.ToString();

            Assert.True(number < double.MaxValue);

            AssertNullConsistency(number);
            Assert.Equal(expectedString, number.ToString());
            Assert.Equal(expectedString, number.ToString("G"));
            Assert.Equal("5.555556E+099", number.ToString("E"));
            Assert.Equal("5.5556E+099", number.ToString("E4"));
        }

        [Fact]
        public void BigDecimal_ToString_VeryLarge()
        {
            const int NumDigits = 10000;
            const int Digit = 5;
            var digitString = Digit.ToString();
            var number = new BigDecimal(BigInteger.Zero, 0);
            var expectedStringBuilder = new StringBuilder(NumDigits);
            for (int i = 0; i < NumDigits; i++)
            {
                number *= 10;
                number += Digit;
                expectedStringBuilder.Append(digitString);
            }

            var expectedString = expectedStringBuilder.ToString();

            Assert.False(number < double.MaxValue);

            AssertNullConsistency(number);
            Assert.Equal(expectedString, number.ToString());
            Assert.Equal(expectedString, number.ToString("G"));
            Assert.Equal("5.555556E+9999", number.ToString("E"));
            Assert.Equal("5.5556E+9999", number.ToString("E4"));
        }

        private static void AssertNullConsistency(BigDecimal number)
        {
            var defaultString = number.ToString();
            Assert.Equal(defaultString, number.ToString((string)null));
            Assert.Equal(defaultString, number.ToString((IFormatProvider)null));
            Assert.Equal(defaultString, number.ToString(null, null));
        }
    }
}
