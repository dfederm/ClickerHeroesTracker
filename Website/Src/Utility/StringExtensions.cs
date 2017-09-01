// <copyright file="StringExtensions.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Utility
{
    using System;

    public static class StringExtensions
    {
        public static string ToCamelCase(this string str)
        {
            if (str == null || str.Length == 0 || char.IsLower(str[0]))
            {
                return str;
            }

            return char.ToLowerInvariant(str[0]) + str.Substring(1);
        }

        public static TEnum SafeParseEnum<TEnum>(this string str)
            where TEnum : struct
        {
            return Enum.TryParse<TEnum>(str, out var value) ? value : default(TEnum);
        }
    }
}