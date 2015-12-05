namespace ClickerHeroesTrackerWebsite.Configuration
{
    public interface IEnvironmentProvider
    {
        string Environment { get; }

        int Changelist { get; }

        string BuildId { get; }
    }
}
