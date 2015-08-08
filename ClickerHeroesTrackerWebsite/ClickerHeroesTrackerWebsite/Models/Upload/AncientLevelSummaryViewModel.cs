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
            var ancientLevels = new List<KeyValuePair<Ancient, int>>(ancientsData.Ancients.Count);
            foreach (var ancientData in ancientsData.Ancients.Values)
            {
                var ancient = Ancient.Get(ancientData.Id);
                if (ancient == null)
                {
                    // An ancient we didn't know about?
                    continue;
                }

                ancientLevels.Add(new KeyValuePair<Ancient, int>(ancient, ancientData.Level));
            }

            this.AncientLevels = ancientLevels.OrderBy(x => x.Key.Name).ToList();
        }

        public IList<KeyValuePair<Ancient, int>> AncientLevels { get; private set; }
    }
}