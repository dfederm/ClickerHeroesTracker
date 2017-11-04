// <copyright file="StringExtensions.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Utility
{
    using System;

    internal static class StringExtensions
    {
        public static TEnum SafeParseEnum<TEnum>(this string str)
            where TEnum : struct
        {
            return Enum.TryParse<TEnum>(str, out var value) ? value : default(TEnum);
        }
    }
}