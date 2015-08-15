namespace ClickerHeroesTrackerWebsite.Models.Home
{
    public class ChangelogViewModel
    {
        public ChangelogViewModel(bool isFull)
        {
            this.IsFull = isFull;
        }

        public bool IsFull { get; private set; }
    }
}