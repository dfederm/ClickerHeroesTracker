namespace ClickerHeroesTrackerWebsite.Models.SaveData
{
    using System.Runtime.Serialization;

    [DataContract]
    public class ItemData
    {
        [DataMember(Name = "name", IsRequired = true)]
        public string Name { get; set; }

        [DataMember(Name = "bonusType1")]
        public int? Bonus1Type { get; set; }

        [DataMember(Name = "bonus1Level")]
        public int? Bonus1Level { get; set; }

        [DataMember(Name = "bonusType2")]
        public int? Bonus2Type { get; set; }

        [DataMember(Name = "bonus2Level")]
        public int? Bonus2Level { get; set; }

        [DataMember(Name = "bonusType3")]
        public int? Bonus3Type { get; set; }

        [DataMember(Name = "bonus3Level")]
        public int? Bonus3Level { get; set; }

        [DataMember(Name = "bonusType4")]
        public int? Bonus4Type { get; set; }

        [DataMember(Name = "bonus4Level")]
        public int? Bonus4Level { get; set; }
    }
}