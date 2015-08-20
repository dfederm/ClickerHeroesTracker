namespace ClickerHeroesTrackerWebsite.Models.SaveData
{
    using Newtonsoft.Json;
    using System.Collections.Generic;

    [JsonObject]
    public class HeroesData
    {
        [JsonProperty(PropertyName = "heroes", Required = Required.Always)]
        public IDictionary<int, HeroData> Heroes { get; set; }
    }
}