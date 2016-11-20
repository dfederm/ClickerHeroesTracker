// <copyright file="DateTimeExtensions.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Utility
{
    using System;
    using System.Collections.Generic;

    internal static class DateTimeExtensions
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

        public static DateTime UnixTimeStampToDateTime(this double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            return javascriptEpoch.AddSeconds(unixTimeStamp);
        }
    }
}