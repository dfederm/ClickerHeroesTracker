// <copyright file="Simulation.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Simulation
{
    using System;
    using System.Collections.Generic;
    using ClickerHeroesTrackerWebsite.Models.Game;
    using ClickerHeroesTrackerWebsite.Models.SaveData;

    public class Simulation
    {
        private static Dictionary<CooldownType, Ancient[]> cooldownTypes = new Dictionary<CooldownType, Ancient[]>
        {
            { CooldownType.Full, new[]
                {
                    Ancient.Chawedo,
                    Ancient.Sniperino,
                    Ancient.Kleptos,
                    Ancient.Berserker,
                    Ancient.Energon,
                    Ancient.Hecatoncheir,
                }
            },
            { CooldownType.Half, new[]
                {
                    Ancient.Chawedo,
                    Ancient.Sniperino,
                    Ancient.Berserker,
                    Ancient.Energon,
                }
            },
            { CooldownType.Short, new[]
                {
                    Ancient.Chawedo,
                    Ancient.Berserker,
                }
            },
            { CooldownType.None, new Ancient[0] }
        };

        private readonly SavedGame savedGame;

        private readonly IDictionary<CooldownType, Activity> activities;

        private readonly IDictionary<Ancient, int> itemLevels;

        public Simulation(SavedGame savedGame, IDictionary<CooldownType, Activity> activities)
        {
            this.savedGame = savedGame;
            this.activities = activities;
            this.itemLevels = this.savedGame.ItemsData.GetItemLevels();
        }

        public SimulateResult Run()
        {
            var factors = this.GetFactors();

            var khrysosLevel = this.savedGame.AncientsData.GetAncientLevel(Ancient.Khrysos);
            var currentGold = khrysosLevel > 0 ? Hero.Get((int)khrysosLevel + 1).Cost : 0;
            var startingZone = 2 + (int)this.GetFactor(Ancient.Iris, 1, 1);
            currentGold += 10
                * MonsterLife(startingZone)
                * MonsterGoldFactor(startingZone)
                * (this.activities == null ? (1 + this.GetFactor(Ancient.Libertas, 0.01, 0.01)) : 1)
                * (1 + this.GetFactor(Ancient.Mammon, 0.05, 0.05));

            var currentTime = 60d;
            var currentSouls = 0d;
            var solomon = 1 + this.GetFactor(Ancient.Solomon, 0.01, 0.01);
            var clicks = 0d;

            var levelingPlan = new LevelingPlan(
                this.savedGame.HeroesData,
                this.GetFactor(Ancient.Argaiv, 0.02, 0.01),
                this.GetFactor(Ancient.Dogcog, 0.02, 0.01));

            SimulateResult best = null;
            for (short level = 5; level <= 4000; level += 5)
            {
                if (level < startingZone)
                {
                    continue;
                }

                var mobs = Math.Min(5, level - (int)this.GetFactor(Ancient.Iris, 1, 1));
                var numDelays = (9 - this.savedGame.AncientsData.GetAncientLevel(Ancient.Kumawakamaru)) * (mobs - 1);
                var levelInfo = this.GetLevelInfo(level, startingZone);

                var addGold = levelInfo.Gold * factors.Gold;
                var curDamage = Math.Max(levelingPlan.GetDamage(currentGold), 10);
                curDamage *= factors.Damage;
                if (this.activities != null)
                {
                    curDamage *= 1 + clicks * this.GetFactor(Ancient.Juggernaut, 0.0001, 0);
                    curDamage *= Math.Pow(1.1, Math.Min(currentTime / 30 - 2, 0));
                }

                var dTime = levelInfo.Life / curDamage;
                currentTime += dTime + numDelays * 0.5;
                currentSouls += this.HeroSoulRewards(level, solomon);
                currentGold += addGold + levelInfo.AvgGold * factors.GoldenClicks * dTime / (dTime + 0.5 * numDelays);
                clicks += dTime * factors.ClickRate;
                if (dTime * factors.ClickRate < numDelays)
                {
                    clicks = 0;
                }

                var realSouls = currentSouls + levelingPlan.GetOptimalHeroSouls(currentGold);
                if (best == null || realSouls / currentTime > best.Ratio)
                {
                    best = new SimulateResult(level, currentTime, realSouls, realSouls / currentTime);
                }
            }

            return best;
        }

        private Factors GetFactors()
        {
            var clickFactor = 0.035 * (1 + this.GetFactor(Ancient.Fragsworth, 0.2, 0.1));
            var critMultiplier = 18 * (1 + this.GetFactor(Ancient.Bhaal, 0.15, 0.01)) - 1;

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
                            avgClickRate += 10 * (30 + this.GetFactor(Ancient.Chawedo, 20, 10)) * count;
                            totalCount += count;
                        }
                    }
                }

                remainsIdle = remains - 60 * Math.Max(totalCount, 2);
            }

            if (remainsClickRate > 1e-3)
            {
                var dDamage = Math.Max(remains, 0) * (1 + (0.09 * critMultiplier + 1) * clickFactor * remainsClickRate);
                avgDamage += dDamage;
                avgGold += dDamage;
                avgClickRate += Math.Max(remains, 0) * remainsClickRate;
            }
            else
            {
                remains = Math.Max(remains, 0);
                remainsIdle = Math.Max(remainsIdle, 0);
                var remainsDiff = remains - remainsIdle;
                var siyaFactor = this.GetFactor(Ancient.Siyalatas, 0.01, 0.1);
                avgDamage += remainsDiff + remainsIdle * (1 + siyaFactor);
                avgGold += remainsDiff + remainsIdle * (1 + siyaFactor) * (1 + this.GetFactor(Ancient.Libertas, 0.01, 0.01));
                avgClickRate = 0;
            }

            avgDamage /= SecsPer30Mins;
            avgGold /= SecsPer30Mins;
            avgGoldenClicks /= SecsPer30Mins;
            avgClickRate /= SecsPer30Mins;

            var damageFactor = 1
                + this.GetFactor(Ancient.Morgulis, 0.11, 0.1)
                + 0.1 * this.savedGame.HeroSouls;
            damageFactor *= this.AchievementDamageMultiplier();
            damageFactor *= HeroUpgrade.MaximumDamageMultiplier;
            if (this.savedGame.HasRubyMultiplier)
            {
                damageFactor *= 2;
            }

            avgDamage *= damageFactor;
            avgGold *= damageFactor;
            var mobTime = (10 - this.savedGame.AncientsData.GetAncientLevel(Ancient.Kumawakamaru)) * 4d;
            var bossTime = 10 * (1 - this.GetFactor(Ancient.Bubos, 0.02, 0.01));
            mobTime /= mobTime + bossTime;
            var goldFactor = 1 + mobTime * 0.01 * (1 + this.GetFactor(Ancient.Dora, 0.2, 0.1)) * (10 * (1 + this.GetFactor(Ancient.Mimzee, 0.5, 0.25)) - 1);
            goldFactor *= 1 + this.GetFactor(Ancient.Mammon, 0.05, 0.05);
            avgGold *= goldFactor;
            avgGoldenClicks *= goldFactor;
            avgGold *= 1 + this.GetFactor(Ancient.Fortuna, 0.0225, 0.0225);

            return new Factors(avgDamage, avgGold / avgDamage, avgGoldenClicks, avgClickRate);
        }

        private double HeroSoulRewards(int level, double solomon)
        {
            if (level == 100)
            {
                return 1;
            }
            else if (level > 100)
            {
                var souls = Math.Floor(Math.Pow(((level - 100) / 5d + 4d) / 5d, 1.3d) * solomon);
                if (level % 100 != 0)
                {
                    souls *= 0.25 + this.GetFactor(Ancient.Atman, 0.01, 0.01);
                }

                return souls;
            }

            return 0;
        }

        private LevelInfo GetLevelInfo(int level, int from)
        {
            var life = 0d;
            var gold = 0d;
            var numMobs = 0;

            var kumawakamaruLevel = this.savedGame.AncientsData.GetAncientLevel(Ancient.Kumawakamaru);
            var startLevel = Math.Max(level - 4, from);
            for (var i = startLevel; i < level; i++)
            {
                var levelLife = MonsterLife(i) * (10 - kumawakamaruLevel);
                life += levelLife;
                gold += levelLife * MonsterGoldFactor(i);
                numMobs += 10 - (int)kumawakamaruLevel;
            }

            var avgGold = gold / Math.Max(numMobs, 1);
            var bossLife = MonsterLife(level) * 10 * (1 - this.GetFactor(Ancient.Bubos, 0.02, 0.01));
            life += bossLife;
            gold += bossLife * MonsterGoldFactor(level);

            return new LevelInfo(life, gold, avgGold);
        }

        private ActivityInfo GetActivityInfo(
            Ancient[] ancients,
            double clickFactor,
            double critMultiplier,
            int clickRate)
        {
            var durations = new List<int>(ancients.Length);
            var cooldowns = new HashSet<Ancient>();

            for (var i = 0; i < ancients.Length; i++)
            {
                durations.Add(this.GetCooldownDuration(ancients[i]));
                cooldowns.Add(ancients[i]);
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
                    var clickSpeed = clickRate + (cooldowns.Contains(Ancient.Chawedo) && this.GetCooldownDuration(Ancient.Chawedo) >= duration ? 10 : 0);
                    var critChance = 0.09 + (cooldowns.Contains(Ancient.Sniperino) && this.GetCooldownDuration(Ancient.Sniperino) >= duration ? 0.50 : 0);
                    var goldenClicks = (cooldowns.Contains(Ancient.Kleptos) && this.GetCooldownDuration(Ancient.Kleptos) >= duration ? clickSpeed * (1 + this.GetFactor(Ancient.Pluto, 0.3, 0.15)) : 0);
                    var powersurge = (cooldowns.Contains(Ancient.Berserker) && this.GetCooldownDuration(Ancient.Berserker) >= duration ? 2 : 1);
                    var goldFactor = (cooldowns.Contains(Ancient.Energon) && this.GetCooldownDuration(Ancient.Energon) >= duration ? 2 : 1);
                    var superClicks = (cooldowns.Contains(Ancient.Hecatoncheir) && this.GetCooldownDuration(Ancient.Hecatoncheir) >= duration ? 3 : 1);
                    var dtime = (duration + 30) - (i > 0 ? (durations[i - 1] + 30) : 0);
                    var damage = dtime * powersurge * (1 + superClicks * (critChance * critMultiplier + 1) * clickSpeed * clickFactor);

                    totalDamage += damage;
                    totalGold += damage * goldFactor;
                    totalGoldenClicks += goldenClicks * goldFactor;
                }
            }

            return new ActivityInfo(totalDamage, totalGold, totalGoldenClicks, duration: durations[durations.Count - 1] + 30);
        }

        private double GetFactor(Ancient ancient, double ancientMultiplier, double itemMultiplier)
        {
            var ancientLevel = this.savedGame.AncientsData.GetAncientLevel(ancient);

            // Adjust ancient levels based on how they scale
            long ancientValue;
            if (ancient == Ancient.Solomon)
            {
                ancientValue = SolomonValue(ancientLevel);
            }
            else if(ancient == Ancient.Siyalatas
                || ancient == Ancient.Libertas)
            {
                ancientValue = IdleValue(ancientLevel);
            }
            else
            {
                ancientValue = ancientLevel;
            }

            var itemLevel = this.itemLevels.GetItemLevel(ancient);

            // Adjust item levels based on how they scale
            int itemValue;
            if (ancient == Ancient.Libertas)
            {
                itemValue = (int)IdleValue(itemLevel);
            }
            else
            {
                itemValue = itemLevel;
            }

            return (ancientMultiplier * ancientValue) + (itemMultiplier * itemValue);
        }

        private int GetCooldownDuration(Ancient ancient)
        {
            // All ancients add 2s and items add 1s. Also, it should always be an integer
            return (int)this.GetFactor(ancient, 2, 1);
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

                    var achievement = Achievement.Get(id);
                    if (achievement != null)
                    {
                        achievementMultiplier *= achievement.Multiplier;
                    }
                }
            }

            return achievementMultiplier;
        }

        private static double AncientPrice(Ancient ancient, long level)
        {
            var price = Math.Round(Math.Pow(level, ancient.Power));
            if (ancient == Ancient.Kumawakamaru)
            {
                price *= 10;
            }

            return price;
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

            return value + add * level;
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

            return value + add * level;
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

        public enum CooldownType
        {
            Full,
            Half,
            Short,
            None
        }

        public sealed class Activity
        {
            public Activity(int count, int clickRate)
            {
                this.Count = count;
                this.ClickRate = clickRate;
            }

            public int Count { get; private set; }

            public int ClickRate { get; private set; }
        }

        public sealed class SimulateResult
        {
            public SimulateResult(short level, double time, double souls, double ratio)
            {
                this.Level = level;
                this.Time = time;
                this.Souls = souls;
                this.Ratio = ratio;
            }

            public short Level { get; private set; }

            public double Time { get; private set; }

            public double Souls { get; private set; }

            public double Ratio { get; private set; }
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

            public double Damage { get; private set; }

            public double Gold { get; private set; }

            public double GoldenClicks { get; private set; }

            public int Duration { get; private set; }
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

            public double Damage { get; private set; }

            public double Gold { get; private set; }

            public double GoldenClicks { get; private set; }

            public double ClickRate { get; private set; }
        }

        private sealed class LevelInfo
        {
            public LevelInfo(double life, double gold, double avgGold)
            {
                this.Life = life;
                this.Gold = gold;
                this.AvgGold = avgGold;
            }

            public double Life { get; private set; }

            public double Gold { get; private set; }

            public double AvgGold { get; private set; }
        }
    }
}