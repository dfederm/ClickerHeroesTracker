namespace ClickerHeroesTrackerWebsite.Models.Calculator
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
                var hero = Hero.Get(heroData.Id);
                if (hero == null)
                {
                    // A hero we didn't know about?
                    continue;
                }

                // No need to show heroes with 0 gilds
                // TODO enable inline expansion
                if (heroData.Gilds > 0)
                {
                    heroGilds.Add(new KeyValuePair<string, string>(hero.Name, heroData.Gilds.ToString()));
                }
            }

            this.HeroGilds = heroGilds;
        }

        public IList<KeyValuePair<string, string>> HeroGilds { get; private set; }
    }
}