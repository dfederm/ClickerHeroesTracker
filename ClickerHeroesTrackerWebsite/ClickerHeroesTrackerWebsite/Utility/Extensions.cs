// <copyright file="Extensions.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models
{
    using System;
    using System.Collections.Generic;

    internal static class Extensions
    {
        private static DateTime javascriptEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long ToJavascriptTime(this DateTime datetime)
        {
            var totalMilliseconds = (long)datetime
                .Subtract(javascriptEpoch)
                .TotalMilliseconds;

            // Truncate to seconds
            totalMilliseconds -= totalMilliseconds % 1000;

            return totalMilliseconds;
        }

        public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dictionay, TKey key, TValue value)
        {
            if (dictionay.ContainsKey(key))
            {
                dictionay[key] = value;
            }
            else
            {
                dictionay.Add(key, value);
            }
        }

        public static TValue SafeGet<TKey, TValue>(this IDictionary<TKey, TValue> dictionay, TKey key)
        {
            TValue value;
            return dictionay.TryGetValue(key, out value) ? value : default(TValue);
        }

        public static TEnum SafeParseEnum<TEnum>(this string str)
            where TEnum : struct
        {
            TEnum value;
            return Enum.TryParse<TEnum>(str, out value) ? value : default(TEnum);
        }

        public static DateTime UnixTimeStampToDateTime(this double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            return javascriptEpoch.AddSeconds(unixTimeStamp);
        }
    }
}