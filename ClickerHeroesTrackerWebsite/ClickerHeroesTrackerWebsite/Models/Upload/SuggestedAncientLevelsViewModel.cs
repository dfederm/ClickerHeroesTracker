namespace ClickerHeroesTrackerWebsite.Models.Upload
{
    using Game;
    using System;
    using System.Collections.Generic;

    public class SuggestedAncientLevelsViewModel
    {
        public SuggestedAncientLevelsViewModel(
            IDictionary<Ancient, int> ancientLevels,
            int optimalLevel,
            UserSettings userSettings)
        {
            var currentSiyaLevel = GetCurrentAncientLevel(ancientLevels, Ancient.Siyalatas);
            var currentArgaivLevel = GetCurrentAncientLevel(ancientLevels, Ancient.Argaiv);
            var currentMorgLevel = GetCurrentAncientLevel(ancientLevels, Ancient.Morgulis);
            var currentLiberLevel = GetCurrentAncientLevel(ancientLevels, Ancient.Libertas);
            var currentMammonLevel = GetCurrentAncientLevel(ancientLevels, Ancient.Mammon);
            var currentMimzeeLevel = GetCurrentAncientLevel(ancientLevels, Ancient.Mimzee);
            var currentIrisLevel = GetCurrentAncientLevel(ancientLevels, Ancient.Iris);
            var currentSolomonLevel = GetCurrentAncientLevel(ancientLevels, Ancient.Solomon);

            var suggestedSiyaLevel = currentSiyaLevel;
            var suggestedArgaivLevel = suggestedSiyaLevel + 9;
            var suggestedMorgLevel = (int)Math.Round(Math.Pow(suggestedSiyaLevel, 2) + (43.67 * suggestedSiyaLevel) + 33.58);
            var suggestedLiberLevel = (int)Math.Round(suggestedSiyaLevel * 0.93);
            var suggestedMammonLevel = suggestedLiberLevel;
            var suggestedMimzeeLevel = suggestedLiberLevel;
            var suggestedIrisLevel = optimalLevel - 1001;
            var suggestedSolomonLevel = userSettings.UseReducedSolomonFormula
                ? (int)Math.Round(1.15 * Math.Pow(Math.Log10(3.25 * Math.Pow(suggestedSiyaLevel, 2)), 0.4) * Math.Pow(suggestedSiyaLevel, 0.8))
                : (int)Math.Round(1.15 * Math.Pow(Math.Log(3.25 * Math.Pow(suggestedSiyaLevel, 2)), 0.4) * Math.Pow(suggestedSiyaLevel, 0.8));

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

        private static int GetCurrentAncientLevel(IDictionary<Ancient, int> ancientLevels, Ancient ancient)
        {
            int level;
            return ancientLevels.TryGetValue(ancient, out level)
                ? level
                : 0;
        }

        public class SuggestedAncientLevelData
        {
            public SuggestedAncientLevelData(Ancient ancient, int currentLevel, int suggestedLevel)
            {
                suggestedLevel = Math.Max(suggestedLevel, 0);

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