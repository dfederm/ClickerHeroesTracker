namespace ClickerHeroesTrackerWebsite.Models.SaveData
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class ItemsData
    {
        [DataMember(Name = "slots", IsRequired = true)]
        public IDictionary<int, int> Slots { get; set; }

        [DataMember(Name = "items", IsRequired = true)]
        public IDictionary<int, ItemData> Items { get; set; }
    }
}