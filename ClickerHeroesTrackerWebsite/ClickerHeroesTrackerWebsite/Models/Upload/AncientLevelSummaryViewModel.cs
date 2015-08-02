namespace ClickerHeroesTrackerWebsite.Models.Upload
{
    using Game;
    using SaveData;
    using System.Collections.Generic;
    using System.Linq;

    public class AncientLevelSummaryViewModel
    {
        public AncientLevelSummaryViewModel(AncientsData ancientsData)
        {
            var ancientLevels = new List<KeyValuePair<string, string>>(ancientsData.Ancients.Count);
            foreach (var ancientData in ancientsData.Ancients.Values)
            {
                var ancient = Ancient.Get(ancientData.Id);
                if (ancient == null)
                {
                    // An ancient we didn't know about?
                    continue;
                }

                ancientLevels.Add(new KeyValuePair<string, string>(ancient.Name, ancientData.Level.ToString()));
            }

            this.AncientLevels = ancientLevels.OrderBy(x => x.Key).ToList();
        }

        public IList<KeyValuePair<string, string>> AncientLevels { get; private set; }
    }
}