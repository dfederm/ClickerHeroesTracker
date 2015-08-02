namespace ClickerHeroesTrackerWebsite.Models.SaveData
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class SavedGame
    {
        [DataMember(Name = "ancients", IsRequired = true)]
        public AncientsData AncientsData { get; set; }

        [DataMember(Name = "heroCollection", IsRequired = true)]
        public HeroesData HeroesData { get; set; }

        [DataMember(Name = "achievements", IsRequired = true)]
        public IDictionary<int, bool> AchievementsData { get; set; }

        [DataMember(Name = "upgrades", IsRequired = true)]
        public IDictionary<int, bool> UpgradeData { get; set; }

        [DataMember(Name = "allDpsMultiplier", IsRequired = true)]
        public double AllDpsMultiplier { get; set; }
    }
}