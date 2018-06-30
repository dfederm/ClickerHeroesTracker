// <copyright file="DateTimeExtensions.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Utility
{
    using System;

    internal static class DateTimeExtensions
    {
        private static readonly DateTime JavascriptEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime UnixTimeStampToDateTime(this double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            return JavascriptEpoch.AddSeconds(unixTimeStamp);
        }
    }
}