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

    /// <summary>
    /// Computes simulation data for a specific set of hero gilds and Argaiv and Dogcog factors.
    /// </summary>
    public sealed class LevelingPlan
    {
        private readonly HeroesData heroesData;

        private readonly double argaivFactor;

        private readonly double dogcogFactor;

        // result plan steps
        private readonly List<PlanStep> plan;

        private readonly double currentCost;

        private readonly Dictionary<int, IList<Upgrade>> heroUpgrades;

        private int planPos;

        private double currentDamage;

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

            this.plan = new List<PlanStep>();

            // Ignore the heroes new to 0.24 due to numbers getting too large. BUGBUG 74: Move to BigInteger.
            var heroes = gameData
                .Heroes
                .Values
                .Where(_ => _.Id <= 35)
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

            // cache of next required hero level (per hero)
            var bestList = new Dictionary<Hero, LevelingPlanInfo>();

            var currentDamage = 0d;
            var baselineLevels = new Dictionary<Hero, int>();

            // At each plan step, we aim to increase the total dps to this multiplier
            var damageTargetRatio = 1.05;

            // Compute baseline damage and levels
            foreach (var hero in heroes)
            {
                // Assume all upgrades can be bought until we hit the rangers. Also, just skip Cid since he doesn't do dps.
                var minimumHeroLevel = 0;
                if (hero.Id != HeroIds.CidtheHelpfulAdventurer && !hero.IsRanger)
                {
                    IList<Upgrade> upgrades;
                    if (this.heroUpgrades.TryGetValue(hero.Id, out upgrades))
                    {
                        minimumHeroLevel = upgrades.Max(_ => _.HeroLevelRequired);
                    }
                }

                currentDamage += this.Damage(hero, minimumHeroLevel);
                baselineLevels.Add(hero, minimumHeroLevel);
            }

            // Compute baseline bestList
            foreach (var hero in heroes)
            {
                var nextBest = this.NextBest(hero, baselineLevels[hero], currentDamage * damageTargetRatio);
                if (nextBest != null)
                {
                    bestList.Add(hero, nextBest);
                }
            }

            // Compute plan steps!
            while (true)
            {
                Hero nextBestHero = null;
                LevelingPlanInfo nextBestTarget = null;

                // Find next target
                foreach (var pair in bestList)
                {
                    var hero = pair.Key;
                    var target = pair.Value;

                    if (nextBestHero == null || target.CostIncrease < nextBestTarget.CostIncrease)
                    {
                        nextBestHero = hero;
                        nextBestTarget = target;
                    }
                }

                if (nextBestHero == null)
                {
                    // Plan over, implies highest hero is maxed
                    break;
                }

                this.plan.Add(new PlanStep(nextBestHero, nextBestTarget.Level));

                // Update target
                currentDamage += nextBestTarget.DamageIncrease;

                if (double.IsInfinity(currentDamage))
                {
                    // Plan over, reached infinity. BUGBUG 74: Move to BigInteger.
                    break;
                }

                // Recompute bestList
                foreach (var hero in heroes)
                {
                    if (bestList.ContainsKey(hero))
                    {
                        var nextBest = this.NextBest(hero, baselineLevels[hero], currentDamage * damageTargetRatio);
                        if (nextBest == null)
                        {
                            bestList.Remove(hero);
                        }
                        else
                        {
                            bestList[hero] = nextBest;
                        }
                    }
                }
            }

            // Fill in real computed plan damage and costs for each step
            this.planPos = 0;
            var heroDamage = new Dictionary<Hero, double>();
            var heroCost = new Dictionary<Hero, double>();
            currentDamage = 0d;
            var currentCost = 0d;
            foreach (var hero in heroes)
            {
                var damage = this.Damage(hero, baselineLevels[hero]);
                currentDamage += damage;
                heroDamage.Add(hero, damage);

                var cost = this.Cost(hero, baselineLevels[hero]);
                currentCost += cost;
                heroCost.Add(hero, cost);
            }

            this.currentDamage = currentDamage;
            this.currentCost = currentCost;
            for (var i = 0; i < this.plan.Count; i++)
            {
                var stepHero = this.plan[i].Hero;

                var nextDamage = this.Damage(stepHero, this.plan[i].Level);
                currentDamage += nextDamage - heroDamage[stepHero];
                heroDamage[stepHero] = nextDamage;
                this.plan[i].Damage = currentDamage;

                var nextCost = this.Cost(stepHero, this.plan[i].Level);
                currentCost += nextCost - heroCost[stepHero];
                heroCost[stepHero] = nextCost;
                this.plan[i].Cost = currentCost;
            }
        }

        /// <summary>
        /// Get the number of souls attainable with the given amount of gold.
        /// This just follows the plan until the gold is expended
        /// </summary>
        /// <returns>The number of souls attainable with this plan and the given gold</returns>
        public double GetOptimalHeroSouls(GameData gameData, double gold)
        {
            Hero frostleaf;
            if (!gameData.Heroes.TryGetValue(HeroIds.Frostleaf, out frostleaf)
                || gold < frostleaf.BaseCost)
            {
                return 0;
            }

            // Close enough for simulation
            return Math.Log(gold) / Math.Log(10) * .25;

            /*
            // The following, while accurate, is very slow!
            var curLevels = [];
            var heroCopy = $.extend(true, { }, Heroes);
            // heroCopy[-1] = { name: "Cid, the Helpful Adventurer", cost: 5, damage: 0, level: 0, upgrades: []};

            // Zero current levels
            for (var i in heroCopy)
            {
                curLevels[i] = 0;
            }

            // Increment cheapest hero by 25 till gold is exhausted
            while (true)
            {
                var cheapest = 0;
                var cheapestCost = 0;
                for (var i in heroCopy)
                {
                    var newCost = this.Cost(heroCopy[i], curLevels[i] + 25);
                    if (cheapestCost == 0 || newCost < cheapestCost)
                    {
                        cheapest = i;
                        cheapestCost = newCost;
                    }
                }

                if (cheapestCost > gold)
                    break;

                gold -= cheapestCost;
                curLevels[cheapest] += 25;
            }

            // Sum levels
            var totalLevels = 0;
            for (var i = 0; i < curLevels.length; i++)
            {
                totalLevels += curLevels[i];
            }

            return totalLevels;
            */
        }

        /// <summary>
        /// Get damage attainable with the given amount of gold.
        /// This just follows the plan until the gold is expended
        /// </summary>
        /// <returns>The damage attainable with this plan and the given gold</returns>
        public double GetDamage(double gold)
        {
            while (this.planPos < this.plan.Count && gold >= this.plan[this.planPos].Cost)
            {
                this.currentDamage = this.plan[this.planPos].Damage;
                this.planPos++;
            }

            if (gold < this.currentCost)
            {
                // Early game (pre-FrostLeaf unlocked)
                return this.currentDamage * Math.Pow(gold / this.currentCost, 0.8);
            }
            else if (this.planPos < this.plan.Count)
            {
                // Mid game (during leveling plan)
                var costRatio = (gold - this.currentCost) / (this.plan[this.planPos].Cost - this.currentCost);
                return this.currentDamage + ((this.plan[this.planPos].Damage - this.currentDamage) * costRatio);
            }
            else
            {
                // Late game (after final hero capped to 4100)
                return this.currentDamage * (1 + (Math.Log(gold / this.currentCost) / (Math.Log(1.07) * 4100)));
            }
        }

        /// <summary>
        /// Compute DPS component of the hero at the given level
        /// </summary>
        /// <returns>The dps of the hero at the given level.</returns>
        private double Damage(Hero hero, int level)
        {
            var x10 = Math.Min(Math.Floor(level / 1000d), 3d);
            var x4 = Math.Min(Math.Max(Math.Floor((level - 175d) / 25d), 0d) - x10, 154d);
            var x5 = hero.IsRanger ? Math.Min(Math.Max(Math.Floor((level - 500d) / 25d), 0d), 9d) : 0d;

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

            return hero.BaseAttack
                * (1 + ((0.5 + this.argaivFactor) * this.heroesData.GetHeroGilds(hero)))
                * level
                * upgradeMultiplier
                * Math.Pow(4, x4)
                * Math.Pow(10, x10)
                * Math.Pow(1.25, x5);
        }

        // Compute cost to level hero to the given level
        private double Cost(Hero hero, int level)
        {
            if (level == 0)
            {
                return 0;
            }

            var cost = hero.BaseCost * Math.Pow(1.07, level) / 0.07 * (1 - this.dogcogFactor);

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

        // Find the next level of the hero that will attain the given target dps
        private LevelingPlanInfo NextBest(Hero hero, int level, double target)
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

            public double Damage { get; set; }

            public double Cost { get; set; }
        }

        private sealed class LevelingPlanInfo
        {
            public LevelingPlanInfo(int level, double damageIncrease, double costIncrese)
            {
                this.Level = level;
                this.DamageIncrease = damageIncrease;
                this.CostIncrease = costIncrese;
            }

            public int Level { get; }

            public double DamageIncrease { get; }

            public double CostIncrease { get; }
        }
    }
}