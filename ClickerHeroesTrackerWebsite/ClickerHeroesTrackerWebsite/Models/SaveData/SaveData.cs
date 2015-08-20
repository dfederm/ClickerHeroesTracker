namespace ClickerHeroesTrackerWebsite.Models.SaveData
{
    using Newtonsoft.Json;
    using System.Collections.Generic;

    [JsonObject]
    public class SavedGame
    {
        [JsonProperty(PropertyName = "ancients", Required = Required.Always)]
        public AncientsData AncientsData { get; set; }

        [JsonProperty(PropertyName = "heroCollection", Required = Required.Always)]
        public HeroesData HeroesData { get; set; }

        [JsonProperty(PropertyName = "items", Required = Required.Always)]
        public ItemsData ItemsData { get; set; }

        [JsonProperty(PropertyName = "achievements", Required = Required.Always)]
        public IDictionary<int, bool> AchievementsData { get; set; }

        [JsonProperty(PropertyName = "upgrades", Required = Required.Always)]
        public IDictionary<int, bool> UpgradeData { get; set; }

        [JsonProperty(PropertyName = "allDpsMultiplier", Required = Required.Always)]
        public double AllDpsMultiplier { get; set; }

        [JsonProperty(PropertyName = "heroSouls", Required = Required.Always)]
        public double HeroSouls { get; set; }

        [JsonProperty(PropertyName = "paidForRubyMultiplier", Required = Required.Always)]
        public bool HasRubyMultiplier { get; set; }
    }
}