namespace ClickerHeroesTrackerWebsite.Models.Upload
{
    using Game;
    using ClickerHeroesTrackerWebsite.Models.SaveData;
    using System;

    public class SuggestedAncientLevelsViewModel
    {
        public SuggestedAncientLevelsViewModel(AncientsData ancientsData)
        {
            var currentSiyaLevel = GetCurrentAncientLevel(ancientsData, Ancient.Siyalatas);
            var currentArgaivLevel = GetCurrentAncientLevel(ancientsData, Ancient.Argaiv);
            var currentMorgLevel = GetCurrentAncientLevel(ancientsData, Ancient.Morgulis);
            var currentLiberLevel = GetCurrentAncientLevel(ancientsData, Ancient.Libertas);
            var currentMammonLevel = GetCurrentAncientLevel(ancientsData, Ancient.Mammon);
            var currentMimzeeLevel = GetCurrentAncientLevel(ancientsData, Ancient.Mimzee);
            var currentIrisLevel = GetCurrentAncientLevel(ancientsData, Ancient.Iris);
            var currentSolomonLevel = GetCurrentAncientLevel(ancientsData, Ancient.Solomon);

            var suggestedSiyaLevel = currentSiyaLevel;
            var suggestedArgaivLevel = suggestedSiyaLevel + 9;
            var suggestedMorgLevel = (int)Math.Round(Math.Pow(suggestedSiyaLevel, 2) + (43.67 * suggestedSiyaLevel) + 33.58);
            var suggestedLiberLevel = (int)Math.Round(suggestedSiyaLevel * 0.93);
            var suggestedMammonLevel = suggestedLiberLevel;
            var suggestedMimzeeLevel = suggestedLiberLevel;
            var suggestedIrisLevel = currentIrisLevel;
            var suggestedSolomonLevel = (int)Math.Round(1.15 * Math.Pow(Math.Log10(3.25 * Math.Pow(suggestedSiyaLevel, 2)), 0.4) * Math.Pow(suggestedSiyaLevel, 0.8));

            this.SuggestedAncientLevels = new SuggestedAncientLevelData[]
            {
                new SuggestedAncientLevelData(Ancient.Siyalatas, currentSiyaLevel, suggestedSiyaLevel),
                new SuggestedAncientLevelData(Ancient.Argaiv, currentArgaivLevel, suggestedArgaivLevel),
                new SuggestedAncientLevelData(Ancient.Morgulis, currentMorgLevel, suggestedMorgLevel),
                new SuggestedAncientLevelData(Ancient.Libertas, currentLiberLevel, suggestedLiberLevel),
                new SuggestedAncientLevelData(Ancient.Mammon, currentMammonLevel, suggestedMammonLevel),
                new SuggestedAncientLevelData(Ancient.Mimzee, currentMimzeeLevel, suggestedMimzeeLevel),
                new SuggestedAncientLevelData(Ancient.Iris, currentIrisLevel, suggestedIrisLevel),
                new SuggestedAncientLevelData(Ancient.Solomon, currentSolomonLevel, suggestedSolomonLevel),
            };
        }

        public SuggestedAncientLevelData[] SuggestedAncientLevels { get; private set; }

        private static int GetCurrentAncientLevel(AncientsData ancientsData, Ancient ancient)
        {
            AncientData ancientData;
            return ancientsData.Ancients.TryGetValue(ancient.Id, out ancientData)
                ? ancientData.Level
                : 0;
        }

        public class SuggestedAncientLevelData
        {
            public SuggestedAncientLevelData(Ancient ancient, int currentLevel, int suggestedLevel)
            {
                this.AncientName = ancient.Name;
                this.CurrentLevel = currentLevel.ToString();
                this.SuggestedLevel = suggestedLevel.ToString();
                this.LevelDifference = (suggestedLevel - currentLevel).ToString();
            }

            public string AncientName { get; private set; }

            public string CurrentLevel { get; private set; }

            public string SuggestedLevel { get; private set; }

            public string LevelDifference { get; private set; }
        }
    }
}