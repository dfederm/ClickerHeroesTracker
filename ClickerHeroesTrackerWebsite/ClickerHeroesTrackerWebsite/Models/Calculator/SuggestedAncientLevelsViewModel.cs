namespace ClickerHeroesTrackerWebsite.Models.Calculator
{
    using Game;
    using System;
    using System.Collections.Generic;

    public class SuggestedAncientLevelsViewModel
    {
        private static ISet<PlayStyle> allPlayStyles = new HashSet<PlayStyle>(new[]
        {
            PlayStyle.Idle,
            PlayStyle.Hybrid,
            PlayStyle.Active,
        });

        private static ISet<PlayStyle> hybridPlayStyles = new HashSet<PlayStyle>(new[]
        {
            PlayStyle.Hybrid,
        });

        public SuggestedAncientLevelsViewModel(
            IDictionary<Ancient, int> ancientLevels,
            int optimalLevel,
            UserSettings userSettings)
        {
            this.UserSettings = userSettings;

            var currentSiyaLevel = GetCurrentAncientLevel(ancientLevels, Ancient.Siyalatas);
            var currentArgaivLevel = GetCurrentAncientLevel(ancientLevels, Ancient.Argaiv);
            var currentMorgLevel = GetCurrentAncientLevel(ancientLevels, Ancient.Morgulis);
            var currentLiberLevel = GetCurrentAncientLevel(ancientLevels, Ancient.Libertas);
            var currentMammonLevel = GetCurrentAncientLevel(ancientLevels, Ancient.Mammon);
            var currentMimzeeLevel = GetCurrentAncientLevel(ancientLevels, Ancient.Mimzee);
            var currentFragsworthLevel = GetCurrentAncientLevel(ancientLevels, Ancient.Fragsworth);
            var currentBhaalLevel = GetCurrentAncientLevel(ancientLevels, Ancient.Bhaal);
            var currentPlutoLevel = GetCurrentAncientLevel(ancientLevels, Ancient.Pluto);
            var currentJuggernautLevel = GetCurrentAncientLevel(ancientLevels, Ancient.Juggernaut);
            var currentIrisLevel = GetCurrentAncientLevel(ancientLevels, Ancient.Iris);
            var currentSolomonLevel = GetCurrentAncientLevel(ancientLevels, Ancient.Solomon);

            var suggestedSiyaLevel = currentSiyaLevel;
            var suggestedArgaivLevel = suggestedSiyaLevel + 9;
            var suggestedMorgLevel = (int)Math.Round(Math.Pow(suggestedSiyaLevel, 2) + (43.67 * suggestedSiyaLevel) + 33.58);
            var suggestedGoldLevel = (int)Math.Round(suggestedSiyaLevel * 0.93);
            var suggestedClickLevel = (int)Math.Round(suggestedSiyaLevel * 0.5);
            var suggestedJuggernautLevel = (int)Math.Round(suggestedClickLevel * 0.2);
            var suggestedIrisLevel = optimalLevel - 1001;
            var suggestedSolomonLevel = userSettings.UseReducedSolomonFormula
                ? (int)Math.Round(1.15 * Math.Pow(Math.Log10(3.25 * Math.Pow(suggestedSiyaLevel, 2)), 0.4) * Math.Pow(suggestedSiyaLevel, 0.8))
                : (int)Math.Round(1.15 * Math.Pow(Math.Log(3.25 * Math.Pow(suggestedSiyaLevel, 2)), 0.4) * Math.Pow(suggestedSiyaLevel, 0.8));

            this.SuggestedAncientLevels = new SuggestedAncientLevelData[]
            {
                new SuggestedAncientLevelData(Ancient.Siyalatas, currentSiyaLevel, suggestedSiyaLevel, allPlayStyles),
                new SuggestedAncientLevelData(Ancient.Argaiv, currentArgaivLevel, suggestedArgaivLevel, allPlayStyles),
                new SuggestedAncientLevelData(Ancient.Morgulis, currentMorgLevel, suggestedMorgLevel, allPlayStyles),
                new SuggestedAncientLevelData(Ancient.Libertas, currentLiberLevel, suggestedGoldLevel, allPlayStyles),
                new SuggestedAncientLevelData(Ancient.Mammon, currentMammonLevel, suggestedGoldLevel, allPlayStyles),
                new SuggestedAncientLevelData(Ancient.Mimzee, currentMimzeeLevel, suggestedGoldLevel, allPlayStyles),
                new SuggestedAncientLevelData(Ancient.Fragsworth, currentFragsworthLevel, suggestedClickLevel, hybridPlayStyles),
                new SuggestedAncientLevelData(Ancient.Bhaal, currentBhaalLevel, suggestedClickLevel, hybridPlayStyles),
                new SuggestedAncientLevelData(Ancient.Pluto, currentPlutoLevel, suggestedClickLevel, hybridPlayStyles),
                new SuggestedAncientLevelData(Ancient.Juggernaut, currentJuggernautLevel, suggestedJuggernautLevel, hybridPlayStyles),
                new SuggestedAncientLevelData(Ancient.Iris, currentIrisLevel, suggestedIrisLevel, allPlayStyles),
                new SuggestedAncientLevelData(Ancient.Solomon, currentSolomonLevel, suggestedSolomonLevel, allPlayStyles),
            };
        }

        public UserSettings UserSettings { get; private set; }

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
            public SuggestedAncientLevelData(
                Ancient ancient,
                int currentLevel,
                int suggestedLevel,
                ISet<PlayStyle> supportedPlayStyles)
            {
                suggestedLevel = Math.Max(suggestedLevel, 0);

                this.AncientName = ancient.Name;
                this.CurrentLevel = currentLevel.ToString();
                this.SuggestedLevel = suggestedLevel.ToString();
                this.LevelDifference = (suggestedLevel - currentLevel).ToString();
                this.SupportedPlayStyles = supportedPlayStyles;
            }

            public string AncientName { get; private set; }

            public string CurrentLevel { get; private set; }

            public string SuggestedLevel { get; private set; }

            public string LevelDifference { get; private set; }

            public ISet<PlayStyle> SupportedPlayStyles { get; private set; }
        }
    }
}