namespace ClickerHeroesTrackerWebsite.Models.Settings
{
    using System;

    public interface IUserSettings
    {
        TimeZoneInfo TimeZone { get; set; }

        bool AreUploadsPublic { get; set; }

        bool UseReducedSolomonFormula { get; set; }

        PlayStyle PlayStyle { get; set; }
    }
}