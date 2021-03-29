// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System;

namespace ClickerHeroesTrackerWebsite.Utility
{
    internal static class DateTimeExtensions
    {
        private static readonly DateTime JavascriptEpoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime UnixTimeStampToDateTime(this double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            return JavascriptEpoch.AddSeconds(unixTimeStamp);
        }
    }
}