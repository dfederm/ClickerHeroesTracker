namespace ClickerHeroesTrackerWebsite.Models.SaveData
{
    using Newtonsoft.Json;

    [JsonObject]
    public class ItemData
    {
        [JsonProperty(PropertyName = "name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "bonusType1")]
        public int? Bonus1Type { get; set; }

        [JsonProperty(PropertyName = "bonus1Level")]
        public int? Bonus1Level { get; set; }

        [JsonProperty(PropertyName = "bonusType2")]
        public int? Bonus2Type { get; set; }

        [JsonProperty(PropertyName = "bonus2Level")]
        public int? Bonus2Level { get; set; }

        [JsonProperty(PropertyName = "bonusType3")]
        public int? Bonus3Type { get; set; }

        [JsonProperty(PropertyName = "bonus3Level")]
        public int? Bonus3Level { get; set; }

        [JsonProperty(PropertyName = "bonusType4")]
        public int? Bonus4Type { get; set; }

        [JsonProperty(PropertyName = "bonus4Level")]
        public int? Bonus4Level { get; set; }
    }
}