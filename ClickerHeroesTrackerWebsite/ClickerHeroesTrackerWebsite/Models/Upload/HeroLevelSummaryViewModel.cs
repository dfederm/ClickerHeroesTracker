namespace ClickerHeroesTrackerWebsite.Models.Upload
{
    using Game;
    using ClickerHeroesTrackerWebsite.Models.SaveData;
    using System.Collections.Generic;

    public class HeroLevelSummaryViewModel
    {
        public HeroLevelSummaryViewModel(HeroesData heroesData)
        {
            var heroGilds = new List<KeyValuePair<string, string>>(heroesData.Heroes.Count);
            foreach (var heroData in heroesData.Heroes.Values)
            {
                var ancient = Hero.Get(heroData.Id);
                if (ancient == null)
                {
                    // A hero we didn't know about?
                    continue;
                }

                heroGilds.Add(new KeyValuePair<string, string>(ancient.Name, heroData.Gilds.ToString()));
            }

            this.HeroGilds = heroGilds;
        }

        public IList<KeyValuePair<string, string>> HeroGilds { get; private set; }
    }
}