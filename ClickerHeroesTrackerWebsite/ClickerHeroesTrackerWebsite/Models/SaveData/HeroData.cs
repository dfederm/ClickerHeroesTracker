namespace ClickerHeroesTrackerWebsite.Models.SaveData
{
    using System.Runtime.Serialization;

    [DataContract]
    public class HeroData
    {
        [DataMember(Name = "id", IsRequired = true)]
        public int Id { get; set; }

        [DataMember(Name = "level", IsRequired = true)]
        public int Level { get; set; }

        [DataMember(Name = "epicLevel", IsRequired = true)]
        public int Gilds { get; set; }
    }
}