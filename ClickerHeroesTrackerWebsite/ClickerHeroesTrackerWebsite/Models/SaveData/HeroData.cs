namespace ClickerHeroesTrackerWebsite.Models.SaveData
{
    using Newtonsoft.Json;

    [JsonObject]
    public class HeroData
    {
        [JsonProperty(PropertyName = "id", Required = Required.Always)]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "level", Required = Required.Always)]
        public int Level { get; set; }

        [JsonProperty(PropertyName = "epicLevel", Required = Required.Always)]
        public int Gilds { get; set; }
    }
}