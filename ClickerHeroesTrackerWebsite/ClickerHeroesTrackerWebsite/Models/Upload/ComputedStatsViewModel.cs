namespace ClickerHeroesTrackerWebsite.Models.Upload
{
    using Game;
    using ClickerHeroesTrackerWebsite.Models.SaveData;

    public class ComputedStatsViewModel
    {
        public ComputedStatsViewModel(SavedGame savedGame)
        {
            /*
  // don't need this?          
  var heroes = data.heroCollection.heroes;
  var ascSouls = 0;
  for (var k in heroes) {
    var id = parseInt(k);
    ascSouls += heroes[k].level;
    if (id < 2 || id > 35) continue;
	Heroes[id - 2].gildBox.val(heroes[k].epicLevel);
  }
  ascSouls = Math.floor(ascSouls / 2000) + data.primalSouls;
  */
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

            /*

            // Maximum possible upgrade multiplier? In case they don't have it currently bought
            for (var k in Upgrades) {
              AchievementMultiplier *= (1 + 0.01 * Upgrades[k]);
            }

            Seed = data.ancients.ancientsRoller.seed;
            var levels = {};
            OwnedNotInList = 0;
            for (var i = AncientMin; i <= AncientMax; i++) {
              if (data.ancients.ancients.hasOwnProperty(i)) {
                levels[i] = true;
                OwnedNotInList += 1;
              }
            }
            for (var k in Ancients) {
              if (levels[Ancients[k].id]) {
                OwnedNotInList -= 1;
              }
            }
            UpdateAncientPrices(levels, data.ancients.didGetVaagur);

            $("#soulsin").val(data.heroSouls + ($("#addsouls").prop("checked") ? ascSouls : 0));
            for (var k in Ancients) {
              if (Ancients.hasOwnProperty(k)) {
                if (data.ancients.ancients[Ancients[k].id]) {
                  Ancients[k].level.val(data.ancients.ancients[Ancients[k].id].level);
                } else {
                  Ancients[k].level.val(0);
                }
              }
            }
            StartCompute();
            */
        }

        public double AchievementMultiplier { get; private set; }

        public double HeroUpgradeMultiplier { get; private set; }

        public double DarkRitualMultiplier { get; private set; }
    }
}