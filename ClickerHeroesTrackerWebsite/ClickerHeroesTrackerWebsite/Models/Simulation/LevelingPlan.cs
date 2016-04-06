// <copyright file="LevelingPlan.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Simulation
{
    using System;
    using System.Collections.Concurrent;
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
        private const int BomberMaxGoldUpgradeLevel = 100;

        private static ConcurrentDictionary<int, BigDecimal> costLevelScaleCache = new ConcurrentDictionary<int, BigDecimal>();

        private static ConcurrentDictionary<int, BigDecimal> levelBonusMultiplierCache = new ConcurrentDictionary<int, BigDecimal>();

        private static double doubleMaxValueSoulsFromHeroLevels = Math.Log(double.MaxValue) / Math.Log(10) * .25;

        private readonly GameData gameData;

        private readonly HeroesData heroesData;

        private readonly double argaivFactor;

        private readonly double dogcogFactor;

        private readonly List<Hero> orderedHeroes;

        private readonly Dictionary<int, IList<Upgrade>> heroUpgrades;

        private readonly Dictionary<Hero, int> currentLevels = new Dictionary<Hero, int>();

        private BigDecimal currentDamage;

        private BigDecimal currentCost;

        private double goldMultiplier = 1;

        private double damageMultiplier = 1;

        private bool isAtRangers;

        /// <summary>
        /// Initializes a new instance of the <see cref="LevelingPlan"/> class.
        /// </summary>
        public LevelingPlan(
            GameData gameData,
            HeroesData heroesData,
            double argaivFactor,
            double dogcogFactor)
        {
            this.gameData = gameData;
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

            // Start all Heroes at level 0
            foreach (var hero in this.orderedHeroes)
            {
                this.currentLevels.Add(hero, 0);
            }
        }

        // Why *2? I dunno.
        private BigDecimal BomberMaxThreshold => this.Cost(this.gameData.Heroes[HeroIds.BomberMax], BomberMaxGoldUpgradeLevel) * 2;

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
        /// Get damage attainable with the given amount of gold after getting all the upgrades up to Frostleaf.
        /// Note, this function will only buy 1 hero at a time and choose the one which can give the most damage.
        /// </summary>
        /// <returns>The damage attainable with this plan and the given gold</returns>
        internal LevelingPlanStep NextStep(BigDecimal gold)
        {
            var bestLevel = 0;
            Hero bestHero = null;

            // Haven't gotten all the Upgrades up to the rangers?
            if (!this.isAtRangers)
            {
                var isDone = true;
                foreach (var hero in this.orderedHeroes)
                {
                    if (hero.IsRanger)
                    {
                        break;
                    }

                    int minimumHeroLevel = 0;
                    IList<Upgrade> upgrades;
                    if (this.heroUpgrades.TryGetValue(hero.Id, out upgrades))
                    {
                        foreach (var upgrade in upgrades)
                        {
                            if (minimumHeroLevel < upgrade.HeroLevelRequired)
                            {
                                minimumHeroLevel = upgrade.HeroLevelRequired;
                            }
                        }
                    }

                    var currentLevel = this.currentLevels[hero];
                    if (currentLevel < minimumHeroLevel)
                    {
                        var level = this.HighestLevelFromHeroGold(hero, gold - this.currentCost + this.Cost(hero, currentLevel));
                        if (level >= minimumHeroLevel)
                        {
                            level = minimumHeroLevel;
                        }
                        else
                        {
                            isDone = false;
                        }

                        if (level > currentLevel)
                        {
                            this.Buy(hero, level);
                        }
                    }
                }

                this.isAtRangers = isDone;
            }

            if (this.isAtRangers)
            {
                var bestNewDmg = this.currentDamage * this.damageMultiplier;
                var remainingGold = gold - this.currentCost;

                foreach (var hero in this.orderedHeroes)
                {
                    var level = this.HighestLevelFromHeroGold(hero, remainingGold + this.Cost(hero, this.currentLevels[hero]));
                    if (level > 25)
                    {
                        // Reduce to multiple of 25 since the extra lvls don't give much DPS compared to cost.
                        level -= level % 25;
                    }

                    var dmgNew = this.NewTotalDamage(hero, level);
                    if (dmgNew > bestNewDmg)
                    {
                        bestLevel = level;
                        bestNewDmg = dmgNew;
                        bestHero = hero;
                    }
                }

                if (bestHero != null)
                {
                    // If the new Hero doesn't contribute 25% damage, then don't buy anything.
                    if ((bestNewDmg - (this.currentDamage * this.damageMultiplier)) / bestNewDmg < 0.25)
                    {
                        bestHero = null;
                        bestLevel = 0;
                    }
                    else
                    {
                        this.Buy(bestHero, bestLevel);
                    }
                }

                // If we didn't buy anything, let's try buying BomberMax's Gold Upgrade.
                var bomberMax = this.gameData.Heroes[HeroIds.BomberMax];
                if ((bestHero == null) && (remainingGold > this.BomberMaxThreshold) && (this.currentLevels[bomberMax] < BomberMaxGoldUpgradeLevel))
                {
                    bestHero = bomberMax;
                    bestLevel = BomberMaxGoldUpgradeLevel;

                    this.Buy(bestHero, bestLevel);
                }
            }

            return new LevelingPlanStep
            {
                Damage = this.currentDamage * this.damageMultiplier,
                GoldMultiplier = this.goldMultiplier,
            };
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

            var levelBonusMultiplier = levelBonusMultiplierCache.GetOrAdd(
                level * (hero.IsRanger ? -1 : 1),
                key =>
                {
                    var isRanger = key < 0;
                    var lvl = Math.Abs(key);

                    var x10 = Math.Min(lvl / 1000, 8);
                    var x4 = Math.Max((lvl - 175) / 25, 0) - x10;
                    var x5 = isRanger ? Math.Min(Math.Max((lvl - 500) / 25, 0), 9) : 0;

                    return BigDecimal.Pow(4, x4)
                        * BigDecimal.Pow(10, x10)
                        * BigDecimal.Pow(1.25, x5);
                });

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

            var gildMultiplier = 1 + (this.argaivFactor * this.heroesData.GetHeroGilds(hero));

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

            // Perf optimization for hot path. Original code:
            // var cost = hero.BaseCost * (BigDecimal.Pow(1.07, level) - 1) / 0.07 * (1 - this.dogcogFactor);
            var levelScale = costLevelScaleCache.GetOrAdd(level, key => (BigDecimal.Pow(1.07, key) - 1) / 0.07);
            var cost = hero.BaseCost * levelScale * (1 - this.dogcogFactor);

            // Assume all upgrades are purchased to the current level
            IList<Upgrade> upgrades;
            if (this.heroUpgrades.TryGetValue(hero.Id, out upgrades))
            {
                var upgradeCost = 0d;
                for (int i = 0; i < upgrades.Count; i++)
                {
                    if (level >= upgrades[i].HeroLevelRequired)
                    {
                        upgradeCost += upgrades[i].GetCost(hero);
                    }
                }

                cost += upgradeCost;
            }

            return cost;
        }

        private int HighestLevelFromHeroGold(Hero hero, BigDecimal gold)
        {
            // Reverse of cost formula (faster and no max level)
            // This is a perf optimization to ensure the mathmatical operations are done in an order that defers the conversion to BigDecimal as long as possible.
            // Original code:
            // var level = Math.Max((int)Math.Floor(BigDecimal.Log(1 + ((gold / hero.BaseCost) * 0.07 / (1 - this.dogcogFactor))) / Math.Log(1.07)), 0);
            var level = Math.Max((int)Math.Floor(BigDecimal.Log(1 + (gold * (0.07 / (hero.BaseCost * (1 - this.dogcogFactor))))) / Math.Log(1.07)), 0);

            // Reduce lvl to account for upgrade costs.
            while (this.Cost(hero, level) > gold)
            {
                level -= 1;
            }

            return level;
        }

        // This function permanently buys a hero updating currentDamage, currentCost, currentLevels and multipliers.
        private void Buy(Hero hero, int newLevel)
        {
            var oldLevel = this.currentLevels[hero];

            this.currentCost += this.Cost(hero, newLevel) - this.Cost(hero, oldLevel);
            this.currentDamage += this.Damage(hero, newLevel) - this.Damage(hero, oldLevel);

            IList<Upgrade> upgrades;
            if (this.heroUpgrades.TryGetValue(hero.Id, out upgrades))
            {
                foreach (var upgrade in upgrades)
                {
                    if (upgrade.HeroLevelRequired <= newLevel && upgrade.HeroLevelRequired > oldLevel)
                    {
                        if (upgrade.UpgradeFunction == UpgradeFunction.UpgradeGoldFoundPercent
                            && upgrade.UpgradeFunctionParameters.Length == 1)
                        {
                            // The param will be like 20 (to denote a 20% increase), so we need to convert that to a multiplier like 1.20
                            this.goldMultiplier *= 1 + (upgrade.UpgradeFunctionParameters[0] / 100);
                        }
                        else if (upgrade.UpgradeFunction == UpgradeFunction.UpgradeEveryonePercent
                            && upgrade.UpgradeFunctionParameters.Length == 1)
                        {
                            // The param will be like 20 (to denote a 20% increase), so we need to convert that to a multiplier like 1.20
                            this.damageMultiplier *= 1 + (upgrade.UpgradeFunctionParameters[0] / 100);
                        }
                    }
                }
            }

            this.currentLevels[hero] = newLevel;
        }

        // Calculates the expected increase in overall damage including damage multipliers (but not gold)
        private BigDecimal NewTotalDamage(Hero hero, int newLevel)
        {
            var oldLevel = this.currentLevels[hero];

            var damageIncrease = this.Damage(hero, newLevel) - this.Damage(hero, oldLevel);
            var newDamageMultiplier = this.damageMultiplier;

            IList<Upgrade> upgrades;
            if (this.heroUpgrades.TryGetValue(hero.Id, out upgrades))
            {
                foreach (var upgrade in upgrades)
                {
                    if (upgrade.HeroLevelRequired <= newLevel && upgrade.HeroLevelRequired > oldLevel)
                    {
                        if (upgrade.UpgradeFunction == UpgradeFunction.UpgradeEveryonePercent
                            && upgrade.UpgradeFunctionParameters.Length == 1)
                        {
                            // The param will be like 20 (to denote a 20% increase), so we need to convert that to a multiplier like 1.20
                            newDamageMultiplier *= 1 + (upgrade.UpgradeFunctionParameters[0] / 100);
                        }
                    }
                }
            }

            return (this.currentDamage + damageIncrease) * newDamageMultiplier;
        }

        internal sealed class LevelingPlanStep
        {
            public BigDecimal Damage { get; set; }

            public double GoldMultiplier { get; set; }
        }
    }
}