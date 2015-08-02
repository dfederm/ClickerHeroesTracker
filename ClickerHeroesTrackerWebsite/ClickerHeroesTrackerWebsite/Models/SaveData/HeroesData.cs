namespace ClickerHeroesTrackerWebsite.Models.SaveData
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class HeroesData
    {
        [DataMember(Name = "heroes", IsRequired = true)]
        public IDictionary<int, HeroData> Heroes { get; set; }
    }
}