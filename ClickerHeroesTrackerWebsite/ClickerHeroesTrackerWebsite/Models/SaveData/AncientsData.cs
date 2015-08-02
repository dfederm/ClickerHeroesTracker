namespace ClickerHeroesTrackerWebsite.Models.SaveData
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class AncientsData
    {
        [DataMember(Name = "ancients", IsRequired = true)]
        public IDictionary<int, AncientData> Ancients { get; set; }
    }
}