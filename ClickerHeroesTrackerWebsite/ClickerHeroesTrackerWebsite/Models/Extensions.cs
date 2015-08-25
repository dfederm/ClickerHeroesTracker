namespace ClickerHeroesTrackerWebsite.Models
{
    using System;
    using System.Collections.Generic;

    public static class Extensions
    {
        private static DateTime JavascriptEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long ToJavascriptTime(this DateTime datetime, TimeZoneInfo timeZone)
        {
            var totalMilliseconds = (long)TimeZoneInfo
                .ConvertTime(datetime, TimeZoneInfo.Utc, timeZone)
                .Subtract(JavascriptEpoch)
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

        public static TEnum SafeParseEnum<TEnum>(this string str) where TEnum : struct
        {
            TEnum value;
            return Enum.TryParse<TEnum>(str, out value) ? value : default(TEnum);
        }
    }
}