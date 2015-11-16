namespace ClickerHeroesTrackerWebsite.Models.Api
{
    public sealed class PaginationMetadata
    {
        public int Count { get; set; }

        public string Previous { get; set; }

        public string Next { get; set; }
    }
}