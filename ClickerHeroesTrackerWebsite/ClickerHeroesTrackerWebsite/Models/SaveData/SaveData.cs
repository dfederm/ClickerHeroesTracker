namespace ClickerHeroesTrackerWebsite.Models.SaveData
{
    using System.Runtime.Serialization;

    [DataContract]
    public class SavedGame
    {
        [DataMember(Name = "ancients", IsRequired = true)]
        public AncientsData AncientsData { get; set; }
    }
}