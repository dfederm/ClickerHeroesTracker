namespace ClickerHeroesTrackerWebsite.Models.SaveData
{
    using Newtonsoft.Json;

    [JsonObject]
    public class AncientData
    {
        [JsonProperty(PropertyName = "id", Required = Required.Always)]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "level", Required = Required.Always)]
        public int Level { get; set; }

        [JsonProperty(PropertyName = "spentHeroSouls", Required = Required.Always)]
        public int SpentHeroSouls { get; set; }
    }
}