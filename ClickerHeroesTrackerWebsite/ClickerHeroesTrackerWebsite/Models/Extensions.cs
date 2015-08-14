namespace ClickerHeroesTrackerWebsite.Models
{
    using System;

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
    }
}