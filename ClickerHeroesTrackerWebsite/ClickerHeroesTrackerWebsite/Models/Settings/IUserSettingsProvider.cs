namespace ClickerHeroesTrackerWebsite.Models.Settings
{
    public interface IUserSettingsProvider
    {
        IUserSettings Get(string userId);

        void FlushChanges();
    }
}
