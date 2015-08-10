namespace ClickerHeroesTrackerWebsite.Models
{
    using System;

    public static class Extensions
    {
        private static DateTime JavascriptEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static double ToJavascriptTime(this DateTime datetime)
        {
            return datetime.Subtract(JavascriptEpoch).TotalMilliseconds;
        }
    }
}