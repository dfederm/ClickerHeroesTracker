// <copyright file="LevelingPlan.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Simulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Game;
    using SaveData;
    using Utility;

    /// <summary>
    /// Computes simulation data for a specific set of hero gilds and Argaiv and Dogcog factors.
    /// </summary>
    public sealed class LevelingPlan
    {
        private static double doubleMaxValueSoulsFromHeroLevels = Math.Log(double.MaxValue) / Math.Log(10) * .25;

        private readonly HeroesData heroesData;

        private readonly double argaivFactor;

        private readonly double dogcogFactor;

        private readonly List<Hero> orderedHeroes;

        private readonly Dictionary<int, IList<Upgrade>> heroUpgrades;

        private readonly Dictionary<Hero, int> currentLevels = new Dictionary<Hero, int>();

        private readonly HashSet<Upgrade> purchasedUpgrades = new HashSet<Upgrade>();

        // The cost for baseline leveling
        private readonly BigDecimal startCost;

        private BigDecimal currentDamage;

        private BigDecimal currentCost;

        private Hero lastHero;

        /// <summary>
        /// Initializes a new instance of the <see cref="LevelingPlan"/> class.
        /// </summary>
        public LevelingPlan(
            GameData gameData,
            HeroesData heroesData,
            double argaivFactor,
            double dogcogFactor)
        {
            this.heroesData = heroesData;
            this.argaivFactor = argaivFactor;
            this.dogcogFactor = dogcogFactor;

            this.orderedHeroes = gameData
                .Heroes
                .Values
                .OrderBy(_ => _.Id)
                .ToList();

            // Get a map of heroes to their upgrades.
            this.heroUpgrades = new Dictionary<int, IList<Upgrade>>(gameData.Heroes.Count);
            foreach (var upgrade in gameData.Upgrades.Values)
            {
                IList<Upgrade> upgrades;
                if (!this.heroUpgrades.TryGetValue(upgrade.HeroId, out upgrades))
                {
                    upgrades = new List<Upgrade>();
                    this.heroUpgrades.Add(upgrade.HeroId, upgrades);
                }

                upgrades.Add(upgrade);
            }

            // Compute baseline damage and levels
            foreach (var hero in this.orderedHeroes)
            {
                // Assume all upgrades can be bought until we hit the rangers. Also, just skip Cid since he doesn't do dps.
                var minimumHeroLevel = 0;
                if (hero.Id != HeroIds.CidtheHelpfulAdventurer && !hero.IsRanger)
                {
                    IList<Upgrade> upgrades;
                    if (this.heroUpgrades.TryGetValue(hero.Id, out upgrades))
                    {
                        foreach (var upgrade in upgrades)
                        {
                            if (minimumHeroLevel < upgrade.HeroLevelRequired)
                            {
                                minimumHeroLevel = upgrade.HeroLevelRequired;
                            }

                            this.purchasedUpgrades.Add(upgrade);
                        }
                    }
                }

                this.currentDamage += this.Damage(hero, minimumHeroLevel);
                this.currentCost += this.Cost(hero, minimumHeroLevel);
                this.currentLevels.Add(hero, minimumHeroLevel);
            }

            this.startCost = this.currentCost;
        }

        /// <summary>
        /// Get the number of souls attainable with the given amount of gold.
        /// This just follows the plan until the gold is expended
        /// </summary>
        /// <returns>The number of souls attainable with this plan and the given gold</returns>
        public double GetSoulsFromHeroLevels(GameData gameData, BigDecimal gold)
        {
            Hero frostleaf;
            if (!gameData.Heroes.TryGetValue(HeroIds.Frostleaf, out frostleaf)
                || gold < frostleaf.BaseCost)
            {
                return 0;
            }

            // Close enough for simulation
            var souls = 0d;
            while (true)
            {
                // Math.Log only takes doubles, so iterate double.MaxValue at a time.
                if (gold > double.MaxValue)
                {
                    souls += doubleMaxValueSoulsFromHeroLevels;
                    gold -= double.MaxValue;
                }
                else
                {
                    souls += Math.Log((double)gold) / Math.Log(10) * .25;
                    break;
                }
            }

            return souls;
        }

        /// <summary>
        /// Get damage attainable with the given amount of gold.
        /// Note, this function will only buy 1 hero at a time and choose the one which CAN give the most damage.
        /// </summary>
        /// <returns>The damage attainable with this plan and the given gold</returns>
        public BigDecimal GetDamage(BigDecimal gold, BigDecimal requestedDamage)
        {
            if (gold < this.startCost)
            {
                // Early game (pre-FrostLeaf unlocked)
                return this.startCost * BigDecimal.Pow((double)(gold / this.startCost), 0.8);
            }
            else
            {
                Hero bestHero = null;
                var bestHeroLevel = 0;
                var bestDamageIncrease = (BigDecimal)0;

                if (this.lastHero != null && this.currentDamage > requestedDamage)
                {
                    bestHero = this.lastHero;
                    bestHeroLevel = this.HighestLevelFromHeroGold(bestHero, gold - this.currentCost + this.Cost(bestHero, this.currentLevels[bestHero]));
                    bestDamageIncrease = this.Damage(bestHero, bestHeroLevel) - this.Damage(bestHero, this.currentLevels[bestHero]);
                }
                else
                {
                    foreach (var hero in this.orderedHeroes)
                    {
                        var currentHeroLevel = this.currentLevels[hero];
                        var newHeroLevel = this.HighestLevelFromHeroGold(hero, gold - this.currentCost + this.Cost(hero, currentHeroLevel));
                        if (newHeroLevel > currentHeroLevel)
                        {
                            var damageIncrease = this.Damage(hero, newHeroLevel) - this.Damage(hero, currentHeroLevel);
                            if (damageIncrease > bestDamageIncrease)
                            {
                                bestHero = hero;
                                bestHeroLevel = newHeroLevel;
                                bestDamageIncrease = damageIncrease;
                            }
                        }
                    }
                }

                this.lastHero = bestHero;
                var damage = this.currentDamage + bestDamageIncrease;

                // Here we cheat to improve the iterative optimizer.  First we give a little bit of extra damage based on unused gold.
                // Note, while this extra damage is used, it's NOT recorded in the plan.
                // 1.07 at the end is because the dmg/cost ratio drops by 1.07 each lvl.  We are using the next lvl ratio which is lower.
                var newCost = this.Cost(bestHero, bestHeroLevel);
                var extraGold = gold - this.currentCost - (newCost - this.Cost(bestHero, this.currentLevels[bestHero]));
                damage += extraGold * (this.Damage(bestHero, bestHeroLevel) / newCost) / 1.07;

                // Now, we reduce the level to be a multiple of 25 since the extra levels didn't give much DPS compared to cost.
                // This reduces the amount of gold wasted on transitional heroes.
                if (this.currentDamage + bestDamageIncrease > requestedDamage)
                {
                    // Gold spent above a multiple of 25 doesn't give much DPS compared to cost.
                    // This reduces the amount of gold wasted on transitional heroes.
                    var tmpLvl = bestHeroLevel - (bestHeroLevel % 25);
                    var curLvl = this.currentLevels[bestHero];
                    while (tmpLvl >= curLvl)
                    {
                        if (tmpLvl == 0)
                        {
                            // Can't buy 0 levels now...
                            tmpLvl = 1;
                        }

                        var tmpDmg = this.Damage(bestHero, tmpLvl) - this.Damage(bestHero, curLvl);
                        if (this.currentDamage + tmpDmg < requestedDamage)
                        {
                            break;
                        }

                        bestHeroLevel = tmpLvl;
                        bestDamageIncrease = tmpDmg;
                        tmpLvl -= 25;
                    }
                }

                this.currentDamage += bestDamageIncrease;
                this.currentCost += this.Cost(bestHero, bestHeroLevel) - this.Cost(bestHero, this.currentLevels[bestHero]);
                this.currentLevels[bestHero] = bestHeroLevel;

                return damage;
            }
        }

        /// <summary>
        /// Compute DPS component of the hero at the given level
        /// </summary>
        /// <returns>The dps of the hero at the given level.</returns>
        private BigDecimal Damage(Hero hero, int level)
        {
            // Hero id 1 is hardcoded in the game code.
            if (level == 0 || hero.Id == 1)
            {
                return 0;
            }

            var x10 = Math.Min(level / 1000, 8);
            var x4 = Math.Max((level - 175) / 25, 0) - x10;
            var x5 = hero.IsRanger ? Math.Min(Math.Max((level - 500) / 25, 0), 9) : 0;
            var levelBonusMultiplier = BigDecimal.Pow(4, x4)
                * BigDecimal.Pow(10, x10)
                * BigDecimal.Pow(1.25, x5);

            var upgradeMultiplier = 1d;
            IList<Upgrade> upgrades;
            if (this.heroUpgrades.TryGetValue(hero.Id, out upgrades))
            {
                for (var i = 0; i < upgrades.Count; i++)
                {
                    // Assume all possible upgrades are purchaded.
                    // Also, we do not handle upgrades purchased on one hero that applies to another specific one.
                    if (upgrades[i].HeroLevelRequired <= level
                        && upgrades[i].UpgradeFunction == UpgradeFunction.UpgradeHeroPercent
                        && upgrades[i].UpgradeFunctionParameters.Length == 2
                        && upgrades[i].UpgradeFunctionParameters[0] == hero.Id)
                    {
                        // The param will be like 20 (to denote a 20% increase), so we need to convert that to a multiplier like 1.20
                        upgradeMultiplier *= 1 + (upgrades[i].UpgradeFunctionParameters[1] / 100);
                    }
                }
            }

            var gildMultiplier = 1 + ((0.5 + this.argaivFactor) * this.heroesData.GetHeroGilds(hero));

            // BaseAttack is severly wrong for the new rangers, so recalculate base attack manually.
            var baseAttack = (hero.BaseCost / 10) * Math.Pow(1 - (0.0188 * Math.Min(hero.Id, 14)), hero.Id);
            if (hero.IsRanger)
            {
                baseAttack *= 5 * Math.Pow(10, (-2 * hero.Id) + 50);
            }

            return baseAttack
                * gildMultiplier
                * level
                * upgradeMultiplier
                * levelBonusMultiplier;
        }

        // Compute cost to level hero to the given level
        private BigDecimal Cost(Hero hero, int level)
        {
            if (level == 0)
            {
                return 0;
            }

            var cost = hero.BaseCost * (BigDecimal.Pow(1.07, level) - 1) / 0.07 * (1 - this.dogcogFactor);

            // Assume all upgrades are purchased to the current level
            IList<Upgrade> upgrades;
            if (this.heroUpgrades.TryGetValue(hero.Id, out upgrades))
            {
                for (int i = 0; i < upgrades.Count; i++)
                {
                    if (level >= upgrades[i].HeroLevelRequired)
                    {
                        cost += upgrades[i].GetCost(hero);
                    }
                }
            }

            return cost;
        }

        private int HighestLevelFromHeroGold(Hero hero, BigDecimal gold)
        {
            // Reverse of cost formula (faster and no max level)
            var level = Math.Max((int)Math.Floor(BigDecimal.Log(1 + ((gold / hero.BaseCost) * 0.07 / (1 - this.dogcogFactor))) / Math.Log(1.07)), 0);

            // Reduce lvl to account for upgrade costs.
            while (this.Cost(hero, level) > gold)
            {
                level -= 1;
            }

            return level;
        }

        // Find the next level of the hero that will attain the given target dps
        private LevelingPlanInfo NextBest(Hero hero, int level, BigDecimal target)
        {
            var currentDamage = this.Damage(hero, level);
            var currentCost = this.Cost(hero, level);

            // Allow single level increments up to level 25
            for (var i = level + 1; i <= 25; i++)
            {
                var newDamage = this.Damage(hero, i);
                if (newDamage < target)
                {
                    continue;
                }

                var newCost = this.Cost(hero, i) - currentCost;
                var newRatio = (newDamage - currentDamage) / (newCost - currentCost);
                return new LevelingPlanInfo(i, newDamage - currentDamage, newCost - currentCost);
            }

            // Ignore leveling past 4100
            for (var i = level + 25; i <= 4100; i += 25)
            {
                var newDamage = this.Damage(hero, i);
                if (newDamage < target)
                {
                    continue;
                }

                var newCost = this.Cost(hero, i) - currentCost;
                var newRatio = (newDamage - currentDamage) / (newCost - currentCost);
                return new LevelingPlanInfo(i, newDamage - currentDamage, newCost - currentCost);
            }

            // unattainable before 4100
            return null;
        }

        private sealed class PlanStep
        {
            public PlanStep(Hero hero, int level)
            {
                this.Hero = hero;
                this.Level = level;
            }

            public Hero Hero { get; }

            public int Level { get; }

            public BigDecimal Damage { get; set; }

            public BigDecimal Cost { get; set; }
        }

        private sealed class LevelingPlanInfo
        {
            public LevelingPlanInfo(int level, BigDecimal damageIncrease, BigDecimal costIncrease)
            {
                this.Level = level;
                this.DamageIncrease = damageIncrease;
                this.CostIncrease = costIncrease;
            }

            public int Level { get; }

            public BigDecimal DamageIncrease { get; }

            public BigDecimal CostIncrease { get; }
        }
    }
}