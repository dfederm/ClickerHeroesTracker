namespace ClickerHeroesTrackerWebsite.Models.SaveData
{
    using Newtonsoft.Json;
    using System.Collections.Generic;

    [JsonObject]
    public class AncientsData
    {
        [JsonProperty(PropertyName = "ancients", Required = Required.Always)]
        public IDictionary<int, AncientData> Ancients { get; set; }
    }
}