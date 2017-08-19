// <copyright file="BigIntegerExtensions.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Utility
{
    using System;
    using System.Globalization;
    using System.Numerics;

    internal static class BigIntegerExtensions
    {
        /// <summary>
        /// Serialize a <see cref="BigInteger"/> to a string that can be used to transport it or store in system which don't support <see cref="BigInteger"/>.
        /// It effectively just calls <see cref="BigInteger.ToString"/> with a specific format that shoudl be used throughout the app.
        /// </summary>
        /// <returns>A string representation of the <see cref="BigInteger"/></returns>
        public static string ToTransportableString(this BigInteger number)
        {
            // Using 24 digits of precision so the string is <= 32 characters if the exponent is at most 4 digits.
            // eg: 1.234567890123456789012346e+9999 => 1 digit + '.' + 24 digits + 'e+' + 4 digits.
            return number.ToString("e18");
        }

        /// <summary>
        /// Deserialize a <see cref="BigInteger"/> from a JSON string. Note that this method handles floating point rounding errors.
        /// </summary>
        /// <returns>A string representation of the <see cref="BigInteger"/></returns>
        public static BigInteger ToBigInteger(this string str)
        {
            if (str.Contains("e"))
            {
                // There's an edge case due to floating point arithmatic that a number which should be an integrer isn't,
                // so we can end up with numbers like 9.250998750999996e9.
                var pieces = str.Split("e");
                var exponent = int.Parse(pieces[1]);
                if (pieces[0].Length - 2 <= exponent)
                {
                    return BigInteger.Parse(str, NumberStyles.Float);
                }
            }

            // Fallback to parsing as a double.
            return new BigInteger(Math.Floor(double.Parse(str)));
        }
    }
}
