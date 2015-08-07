namespace ClickerHeroesTrackerWebsite.Models.Upload
{
    using Game;
    using ClickerHeroesTrackerWebsite.Models.SaveData;
    using ClickerHeroesTrackerWebsite.Models.Simulation;
    using System;

    public class ComputedStatsViewModel
    {
        public ComputedStatsViewModel(SavedGame savedGame)
        {
            var achievementMultiplier = 1d;
            foreach (var pair in savedGame.AchievementsData)
            {
                var id = pair.Key;
                var haveAchievement = pair.Value;
                if (!haveAchievement)
                {
                    continue;
                }

                var achievement = Achievement.Get(id);
                if (achievement != null)
                {
                    achievementMultiplier *= achievement.Multiplier;
                }
            }

            this.AchievementMultiplier = achievementMultiplier;

            var upgradeMultiplier = 1d;
            foreach (var pair in savedGame.UpgradeData)
            {
                var id = pair.Key;
                var haveUpgrade = pair.Value;
                if (!haveUpgrade)
                {
                    continue;
                }

                var upgrade = HeroUpgrade.Get(id);
                if (upgrade != null)
                {
                    upgradeMultiplier *= upgrade.DamageMultiplier;
                }
            }

            this.HeroUpgradeMultiplier = upgradeMultiplier;

            this.DarkRitualMultiplier = savedGame.AllDpsMultiplier / (achievementMultiplier * upgradeMultiplier);

            // No activities for now; assume idle mode
            var simulationResult = new Simulation(savedGame, null).Run();

            this.SoulsPerHour = Math.Round(simulationResult.Ratio * 3600);
            this.OptimalLevel = simulationResult.Level;
            this.OptimalSoulsPerAscension = Math.Round(simulationResult.Souls);
            this.OptimalAscensionTime = (int)Math.Round(simulationResult.Time / 60);
        }

        public double AchievementMultiplier { get; private set; }

        public double HeroUpgradeMultiplier { get; private set; }

        public double DarkRitualMultiplier { get; private set; }

        public double SoulsPerHour { get; private set; }

        public int OptimalLevel { get; private set; }

        public double OptimalSoulsPerAscension { get; private set; }

        public int OptimalAscensionTime { get; private set; }
    }
}