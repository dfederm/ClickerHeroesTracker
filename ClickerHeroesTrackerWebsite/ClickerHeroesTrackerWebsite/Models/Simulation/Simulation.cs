// <copyright file="Simulation.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Simulation
{
    using System;
    using System.Collections.Generic;
    using ClickerHeroesTrackerWebsite.Models.Game;
    using ClickerHeroesTrackerWebsite.Models.SaveData;
    using Utility;

    /// <summary>
    /// Simulates a run.
    /// </summary>
    public class Simulation
    {
        private static Dictionary<CooldownType, int[]> cooldownTypes = new Dictionary<CooldownType, int[]>
        {
            {
                CooldownType.Full, new[]
                {
                    AncientIds.Chawedo,
                    AncientIds.Sniperino,
                    AncientIds.Kleptos,
                    AncientIds.Berserker,
                    AncientIds.Energon,
                    AncientIds.Hecatoncheir,
                }
            },
            {
                CooldownType.Half, new[]
                {
                    AncientIds.Chawedo,
                    AncientIds.Sniperino,
                    AncientIds.Berserker,
                    AncientIds.Energon,
                }
            },
            {
                CooldownType.Short, new[]
                {
                    AncientIds.Chawedo,
                    AncientIds.Berserker,
                }
            },
            { CooldownType.None, new int[0] }
        };

        private readonly GameData gameData;

        private readonly SavedGame savedGame;

        private readonly IDictionary<CooldownType, CooldownActivity> activities;

        private readonly IDictionary<int, int> itemLevels;

        /// <summary>
        /// Initializes a new instance of the <see cref="Simulation"/> class.
        /// </summary>
        public Simulation(
            GameData gameData,
            SavedGame savedGame,
            IDictionary<CooldownType, CooldownActivity> activities)
        {
            this.gameData = gameData;
            this.savedGame = savedGame;
            this.activities = activities;
            this.itemLevels = this.savedGame.ItemsData.GetItemLevels();
        }

        /// <summary>
        /// The cooldown type the user plans to use.
        /// </summary>
        public enum CooldownType
        {
            /// <summary>
            /// Use cooldowns 1+2+3+4+5+7
            /// </summary>
            Full,

            /// <summary>
            /// Use cooldowns 1+2+3+4
            /// </summary>
            Half,

            /// <summary>
            /// Use cooldowns 1+2
            /// </summary>
            Short,

            /// <summary>
            /// No cooldowns
            /// </summary>
            None
        }

        /// <summary>
        /// Runs the simulation.
        /// </summary>
        /// <remarks>
        /// Based on http://philni.neocities.org/ancientsworker10.js?ver=8
        /// </remarks>
        /// <returns>The simulation result</returns>
        public SimulateResult Run()
        {
            var factors = this.GetFactors();

            Hero hero;
            var khrysosLevel = this.savedGame.AncientsData.GetAncientLevel(AncientIds.Khrysos);
            var currentGold = khrysosLevel > 0 && this.gameData.Heroes.TryGetValue((int)khrysosLevel + 1, out hero)
                ? (BigDecimal)hero.BaseCost
                : 0;
            var startingZone = 1 + (int)this.GetFactor(AncientIds.Iris, 1, 1);
            currentGold += 10
                * MonsterLife(startingZone)
                * MonsterGoldFactor(startingZone)
                * (this.activities == null ? (1 + this.GetFactor(AncientIds.Libertas, 0.01, 0.01)) : 1)
                * (1 + this.GetFactor(AncientIds.Mammon, 0.05, 0.05));

            const double SetupTime = 60d;

            var currentTime = SetupTime;
            var currentSouls = 0d;
            var solomon = 1 + this.GetFactor(AncientIds.Solomon, 0.01, 0.01);
            var clicks = 0d;

            var levelingPlan = new LevelingPlan(
                this.gameData,
                this.savedGame.HeroesData,
                this.GetFactor(AncientIds.Argaiv, 0.02, 0.01),
                this.GetFactor(AncientIds.Dogcog, 0.02, 0.01));

            SimulateResult best = null;
            for (var level = startingZone + 4 - ((startingZone - 1) % 5); level <= 4500; level += 5)
            {
                var waves = Math.Min(5, level + 1 - startingZone);
                var kumawakamaruLevel = this.savedGame.AncientsData.GetAncientLevel(AncientIds.Kumawakamaru);
                var numDelays = (9 - kumawakamaruLevel) * (waves - 1);
                var levelInfo = this.GetLevelInfo(level, startingZone);

                var curDamage = levelingPlan.GetDamage(currentGold, (levelInfo.BossLife * 30) / factors.Damage);
                if (curDamage < 10)
                {
                    curDamage = 10;
                }

                var bossTime = levelInfo.BossLife / (curDamage * factors.Damage);
                if (best != null && bossTime > (30 + this.GetFactor(AncientIds.Chronos, 5, 5)))
                {
                    break;
                }

                curDamage *= factors.Damage;
                if (this.activities != null)
                {
                    curDamage *= 1 + (clicks * this.GetFactor(AncientIds.Juggernaut, 0.0001, 0));
                    curDamage *= Math.Pow(1.1, Math.Min((currentTime / 30) - 2, 0));
                }

                var time = (double)(levelInfo.Life / curDamage);
                currentTime += time + (numDelays * 0.5);
                currentSouls += this.HeroSoulRewards(level, solomon);
                var addGold = levelInfo.Gold * factors.Gold;
                currentGold += addGold + (levelInfo.AvgGold * factors.GoldenClicks * time / (time + (0.5 * numDelays)));
                clicks += time * factors.ClickRate;
                if (time * factors.ClickRate < numDelays)
                {
                    clicks = 0;
                }

                var realSouls = currentSouls + levelingPlan.GetSoulsFromHeroLevels(this.gameData, currentGold);
                var ratio = realSouls / currentTime;
                if (best == null || ratio > best.Ratio)
                {
                    best = new SimulateResult(level, currentTime, realSouls, ratio);
                }
                else if (ratio < best.Ratio * .80)
                {
                    // ratio down to 80%, it won't get better, stop simulation
                    break;
                }
            }

            return best;
        }

        private static long IdleValue(long level)
        {
            var value = 0;
            var add = 25;

            // Only the first 9 levels get the 25% bonus
            if (level > 9)
            {
                level -= 9;
                value = add * 9;
                add = 24;
            }

            // Bonus decreases 1% every 10 levels
            while (add > 15 && level > 10)
            {
                value += add * 10;
                add -= 1;
                level -= 10;
            }

            return value + (add * level);
        }

        private static long SolomonValue(long level)
        {
            var value = 0;
            var add = 5;
            while (add > 1 && level > 20)
            {
                value += add * 20;
                add -= 1;
                level -= 20;
            }

            return value + (add * level);
        }

        private static double MonsterLife(int level)
        {
            return 10
                * (Math.Pow(1.6, Math.Min(level, 140) - 1) + Math.Min(level, 140) - 1)
                * Math.Pow(1.15, Math.Max(level - 140, 0));
        }

        private static double MonsterGoldFactor(int level)
        {
            var factor = 1.0 / 15;
            if (level > 75)
            {
                factor *= Math.Min(3, Math.Pow(1.025, level - 75));
            }

            return factor;
        }

        private Factors GetFactors()
        {
            var clickFactor = 0.035 * (1 + this.GetFactor(AncientIds.Fragsworth, 0.2, 0.1));
            var critMultiplier = (18 * (1 + this.GetFactor(AncientIds.Bhaal, 0.15, 0.01))) - 1;

            var avgDamage = 0d;
            var avgGold = 0d;
            var avgGoldenClicks = 0d;

            const double SecsPer30Mins = 1800d;
            var remains = SecsPer30Mins;
            var remainsIdle = SecsPer30Mins;
            var remainsClickRate = 0d;
            var avgClickRate = 0d;

            if (this.activities != null)
            {
                var totalCount = 0;
                foreach (var pair in this.activities)
                {
                    var cooldownType = pair.Key;
                    var activity = pair.Value;

                    if (cooldownType == CooldownType.None)
                    {
                        remainsClickRate = activity.ClickRate;
                    }
                    else
                    {
                        var count = activity.Count;
                        if (count > 1e-3)
                        {
                            var info = this.GetActivityInfo(
                                cooldownTypes[cooldownType],
                                clickFactor,
                                critMultiplier,
                                activity.ClickRate);
                            avgDamage += info.Damage * count;
                            avgGold += info.Gold * count;
                            avgGoldenClicks += info.GoldenClicks * count;
                            remains -= info.Duration * count;
                            avgClickRate += activity.ClickRate * info.Duration * count;
                            avgClickRate += 10 * (30 + this.GetFactor(AncientIds.Chawedo, 20, 10)) * count;
                            totalCount += count;
                        }
                    }
                }

                remainsIdle = remains - (60 * Math.Max(totalCount, 2));
            }

            if (remainsClickRate > 1e-3)
            {
                var damage = Math.Max(remains, 0) * (1 + (((0.09 * critMultiplier) + 1) * clickFactor * remainsClickRate));
                avgDamage += damage;
                avgGold += damage;
                avgClickRate += Math.Max(remains, 0) * remainsClickRate;
            }
            else
            {
                remains = Math.Max(remains, 0);
                remainsIdle = Math.Max(remainsIdle, 0);
                var remainsDiff = remains - remainsIdle;
                var siyaFactor = this.GetFactor(AncientIds.Siyalatas, 0.01, 0.1);
                avgDamage += remainsDiff + (remainsIdle * (1 + siyaFactor));
                avgGold += remainsDiff + (remainsIdle * (1 + siyaFactor) * (1 + this.GetFactor(AncientIds.Libertas, 0.01, 0.01)));
                avgClickRate = 0;
            }

            avgDamage /= SecsPer30Mins;
            avgGold /= SecsPer30Mins;
            avgGoldenClicks /= SecsPer30Mins;
            avgClickRate /= SecsPer30Mins;

            var damageFactor = 1
                + this.GetFactor(AncientIds.Morgulis, 0.11, 0.1)
                + (0.1 * this.savedGame.HeroSouls);
            damageFactor *= this.AchievementDamageMultiplier();

            // Assume all upgrades are reachable up until the rangers.
            foreach (var upgrade in this.gameData.Upgrades.Values)
            {
                Hero hero;
                if (upgrade.UpgradeFunction == UpgradeFunction.UpgradeEveryonePercent
                    && upgrade.UpgradeFunctionParameters.Length == 1
                    && this.gameData.Heroes.TryGetValue(upgrade.HeroId, out hero)
                    && !hero.IsRanger)
                {
                    // The param will be like 20 (to denote a 20% increase), so we need to convert that to a multiplier like 1.20
                    damageFactor *= 1 + (upgrade.UpgradeFunctionParameters[0] / 100d);
                }
            }

            if (this.savedGame.HasRubyMultiplier)
            {
                damageFactor *= 2;
            }

            avgDamage *= damageFactor;
            avgGold *= damageFactor;
            var mobTime = (10 - this.savedGame.AncientsData.GetAncientLevel(AncientIds.Kumawakamaru)) * 4d;
            var bossTime = 10 * (1 - this.GetFactor(AncientIds.Bubos, 0.02, 0.01));
            mobTime /= mobTime + bossTime;
            var goldFactor = 1 + (mobTime * 0.01 * (1 + this.GetFactor(AncientIds.Dora, 0.2, 0.1)) * ((10 * (1 + this.GetFactor(AncientIds.Mimzee, 0.5, 0.25))) - 1));
            goldFactor *= 1 + this.GetFactor(AncientIds.Mammon, 0.05, 0.05);
            avgGoldenClicks *= goldFactor;

            // Midas upgrades
            goldFactor *= 1.25 * 1.25 * 1.25 * 1.5; // Midas
            goldFactor *= 1 + this.GetFactor(AncientIds.Fortuna, 0.0225, 0.0225);
            avgGold *= goldFactor;

            return new Factors(avgDamage, avgGold / avgDamage, avgGoldenClicks / 100, avgClickRate);
        }

        private double HeroSoulRewards(int level, double solomon)
        {
            if (level == 100)
            {
                return 1;
            }
            else if (level > 100)
            {
                var souls = Math.Floor(Math.Pow((((level - 100) / 5d) + 4d) / 5d, 1.3d) * solomon);
                if (level % 100 != 0)
                {
                    souls *= 0.25 + this.GetFactor(AncientIds.Atman, 0.01, 0.01);
                }

                return souls;
            }

            return 0;
        }

        private LevelInfo GetLevelInfo(int level, int from)
        {
            BigDecimal life = 0;
            BigDecimal gold = 0;
            var numMobs = 0;

            var numMobsPerLevel = 10 - (int)this.savedGame.AncientsData.GetAncientLevel(AncientIds.Kumawakamaru);
            var startLevel = Math.Max(level - 4, from);
            for (var i = startLevel; i < level; i++)
            {
                var levelLife = MonsterLife(i) * numMobsPerLevel;
                life += levelLife;
                gold += levelLife * MonsterGoldFactor(i);
                numMobs += numMobsPerLevel;
            }

            var bossLife = MonsterLife(level) * 10 * (1 - this.GetFactor(AncientIds.Bubos, 0.02, 0.01));
            var bossGold = bossLife * MonsterGoldFactor(level);
            var avgGold = (gold + (bossGold / 10)) / (numMobs + 1);

            life += bossLife;
            gold += bossGold;

            return new LevelInfo(life, gold, avgGold, bossLife);
        }

        private ActivityInfo GetActivityInfo(
            int[] ancientIds,
            double clickFactor,
            double critMultiplier,
            int clickRate)
        {
            var durations = new List<int>(ancientIds.Length);
            var cooldowns = new HashSet<int>();

            for (var i = 0; i < ancientIds.Length; i++)
            {
                durations.Add(this.GetCooldownDuration(ancientIds[i]));
                cooldowns.Add(ancientIds[i]);
            }

            durations.Sort();

            var totalDamage = 0d;
            var totalGold = 0d;
            var totalGoldenClicks = 0d;

            for (var i = 0; i < durations.Count; i++)
            {
                var duration = durations[i];
                if (i == 0 || duration > durations[i - 1])
                {
                    var clickSpeed = clickRate + (cooldowns.Contains(AncientIds.Chawedo) && this.GetCooldownDuration(AncientIds.Chawedo) >= duration ? 10 : 0);
                    var critChance = 0.09 + (cooldowns.Contains(AncientIds.Sniperino) && this.GetCooldownDuration(AncientIds.Sniperino) >= duration ? 0.50 : 0);
                    var goldenClicks = cooldowns.Contains(AncientIds.Kleptos) && this.GetCooldownDuration(AncientIds.Kleptos) >= duration ? clickSpeed * (1 + this.GetFactor(AncientIds.Pluto, 0.3, 0.15)) : 0;
                    var powersurge = cooldowns.Contains(AncientIds.Berserker) && this.GetCooldownDuration(AncientIds.Berserker) >= duration ? 2 : 1;
                    var goldFactor = cooldowns.Contains(AncientIds.Energon) && this.GetCooldownDuration(AncientIds.Energon) >= duration ? 2 : 1;
                    var superClicks = cooldowns.Contains(AncientIds.Hecatoncheir) && this.GetCooldownDuration(AncientIds.Hecatoncheir) >= duration ? 3 : 1;
                    var dtime = (duration + 30) - (i > 0 ? (durations[i - 1] + 30) : 0);
                    var damage = dtime * powersurge * (1 + (superClicks * ((critChance * critMultiplier) + 1) * clickSpeed * clickFactor));

                    totalDamage += damage;
                    totalGold += damage * goldFactor;
                    totalGoldenClicks += goldenClicks * goldFactor;
                }
            }

            return new ActivityInfo(totalDamage, totalGold, totalGoldenClicks, duration: durations[durations.Count - 1] + 30);
        }

        private double GetFactor(int ancientId, double ancientMultiplier, double itemMultiplier)
        {
            Ancient ancient;
            var ancientLevel = this.gameData.Ancients.TryGetValue(ancientId, out ancient)
                ? this.savedGame.AncientsData.GetAncientLevel(ancientId)
                : 0;

            // Adjust ancient levels based on how they scale
            long ancientValue;
            if (ancientId == AncientIds.Solomon)
            {
                ancientValue = SolomonValue(ancientLevel);
            }
            else if (ancientId == AncientIds.Siyalatas
                || ancientId == AncientIds.Libertas)
            {
                ancientValue = IdleValue(ancientLevel);
            }
            else
            {
                ancientValue = ancientLevel;
            }

            var itemLevel = this.itemLevels.GetItemLevel(ancientId);

            // Adjust item levels based on how they scale
            int itemValue;
            if (ancientId == AncientIds.Libertas)
            {
                itemValue = (int)IdleValue(itemLevel);
            }
            else
            {
                itemValue = itemLevel;
            }

            return (ancientMultiplier * ancientValue) + (itemMultiplier * itemValue);
        }

        private int GetCooldownDuration(int ancientId)
        {
            // All ancients add 2s and items add 1s. Also, it should always be an integer
            return (int)this.GetFactor(ancientId, 2, 1);
        }

        private double AchievementDamageMultiplier()
        {
            var achievementMultiplier = 1d;
            if (this.savedGame.AchievementsData != null)
            {
                foreach (var pair in this.savedGame.AchievementsData)
                {
                    var id = pair.Key;
                    var haveAchievement = pair.Value;
                    if (!haveAchievement)
                    {
                        continue;
                    }

                    Achievement achievement;
                    if (this.gameData.Achievements.TryGetValue(id, out achievement)
                        && achievement.RewardFunction == RewardFunction.AddDps
                        && achievement.RewardFunctionParams.Length == 1)
                    {
                        // The param will be like 5 (to denote a 5% increase), so we need to convert that to a multiplier like 1.05
                        achievementMultiplier *= 1 + (achievement.RewardFunctionParams[0] / 100d);
                    }
                }
            }

            return achievementMultiplier;
        }

        /// <summary>
        /// Represents a cooldown activity
        /// </summary>
        public sealed class CooldownActivity
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CooldownActivity"/> class.
            /// </summary>
            public CooldownActivity(int count, int clickRate)
            {
                this.Count = count;
                this.ClickRate = clickRate;
            }

            /// <summary>
            /// Gets the number of times per 30 minutes the cooldown will be used.
            /// </summary>
            public int Count { get; }

            /// <summary>
            /// Gets the number of clicks per second during this cooldown.
            /// </summary>
            public int ClickRate { get; }
        }

        /// <summary>
        /// The result of the simulation.
        /// </summary>
        public sealed class SimulateResult
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SimulateResult"/> class.
            /// </summary>
            public SimulateResult(int level, double time, double souls, double ratio)
            {
                this.Level = level;
                this.Time = time;
                this.Souls = souls;
                this.Ratio = ratio;
            }

            /// <summary>
            /// Gets the level the simulation made it to.
            /// </summary>
            public int Level { get; }

            /// <summary>
            /// Gets the time it will take to get to the simulated level.
            /// </summary>
            public double Time { get; }

            /// <summary>
            /// Gets the expected total number of souls expected at the simulated level.
            /// </summary>
            public double Souls { get; }

            /// <summary>
            /// Gets the ratio of expected souls to the expected time.
            /// </summary>
            /// <remarks>
            /// This is used to determine the efficiency of a simulation.
            /// </remarks>
            public double Ratio { get; }
        }

        private sealed class ActivityInfo
        {
            public ActivityInfo(double damage, double gold, double goldenClicks, int duration)
            {
                this.Damage = damage;
                this.Gold = gold;
                this.GoldenClicks = goldenClicks;
                this.Duration = duration;
            }

            public double Damage { get; }

            public double Gold { get; }

            public double GoldenClicks { get; }

            public int Duration { get; }
        }

        private sealed class Factors
        {
            public Factors(double damage, double gold, double goldenClicks, double clickRate)
            {
                this.Damage = damage;
                this.Gold = gold;
                this.GoldenClicks = goldenClicks;
                this.ClickRate = clickRate;
            }

            public double Damage { get; }

            public double Gold { get; }

            public double GoldenClicks { get; }

            public double ClickRate { get; }
        }

        private sealed class LevelInfo
        {
            public LevelInfo(
                BigDecimal life,
                BigDecimal gold,
                BigDecimal avgGold,
                BigDecimal bossLife)
            {
                this.Life = life;
                this.Gold = gold;
                this.AvgGold = avgGold;
                this.BossLife = bossLife;
            }

            public BigDecimal Life { get; }

            public BigDecimal Gold { get; }

            public BigDecimal AvgGold { get; }

            public BigDecimal BossLife { get; }
        }
    }
}