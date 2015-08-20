namespace ClickerHeroesTrackerWebsite.Models.SaveData
{
    using Newtonsoft.Json;
    using System.Collections.Generic;

    [JsonObject]
    public class ItemsData
    {
        [JsonProperty(PropertyName = "slots", Required = Required.Always)]
        public IDictionary<int, int> Slots { get; set; }

        [JsonProperty(PropertyName = "items", Required = Required.Always)]
        public IDictionary<int, ItemData> Items { get; set; }
    }
}