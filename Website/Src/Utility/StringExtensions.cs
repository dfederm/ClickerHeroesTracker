// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System;

namespace ClickerHeroesTrackerWebsite.Utility
{
    internal static class StringExtensions
    {
        public static TEnum SafeParseEnum<TEnum>(this string str)
            where TEnum : struct
        {
            return Enum.TryParse(str, out TEnum value) ? value : default(TEnum);
        }
    }
}