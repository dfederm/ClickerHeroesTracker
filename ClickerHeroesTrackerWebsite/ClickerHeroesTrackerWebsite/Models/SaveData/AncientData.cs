namespace ClickerHeroesTrackerWebsite.Models.SaveData
{
    using System.Runtime.Serialization;

    [DataContract]
    public class AncientData
    {
        [DataMember(Name = "id", IsRequired = true)]
        public int Id { get; set; }

        [DataMember(Name = "level", IsRequired = true)]
        public int Level { get; set; }
    }
}