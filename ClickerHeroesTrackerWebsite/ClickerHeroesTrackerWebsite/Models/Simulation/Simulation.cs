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
        private const int ActiveClicksPerSecond = 40;

        private readonly GameData gameData;

        private readonly SavedGame savedGame;

        private readonly PlayStyle playStyle;

        private readonly IDictionary<int, int> itemLevels;

        /// <summary>
        /// Initializes a new instance of the <see cref="Simulation"/> class.
        /// </summary>
        public Simulation(
            GameData gameData,
            SavedGame savedGame,
            PlayStyle playStyle)
        {
            this.gameData = gameData;
            this.savedGame = savedGame;
            this.playStyle = playStyle;

            this.itemLevels = this.savedGame.ItemsData.GetItemLevels();
        }

        /// <summary>
        /// Runs the simulation.
        /// </summary>
        /// <remarks>
        /// Based on http://philni.neocities.org/ancientsworker10.js?ver=16 and http://philni.neocities.org/shared.js?ver=17
        /// </remarks>
        /// <returns>The simulation result</returns>
        public SimulateResult Run()
        {
            // Clicks and damage
            var clicks = 0;
            var accumulatorCrit = 0.50;
            var damageFactor = 1
                + this.GetFactor(AncientIds.Morgulis, 0.11, 0.1)
                + (0.1 * this.savedGame.HeroSouls);
            damageFactor *= this.AchievementDamageMultiplier();
            if (this.savedGame.HasRubyMultiplier)
            {
                damageFactor *= 2;
            }

            // Start Level
            var startingZone = 1 + (int)this.GetFactor(AncientIds.Iris, 1, 1);

            // Time
            const double SetupTime = 60d;
            var currentTime = SetupTime;
            var timeUntilIdle = currentTime + 60;
            if (this.playStyle == PlayStyle.Idle)
            {
                // Start Idle right away
                timeUntilIdle = 0;
            }
            else
            {
                // With a pure clickable start after hybrid/active, we need to add 60s so the Libertas bonus will apply the clickable.
                currentTime += 60;
            }

            // Gold
            var khrysosLevel = this.savedGame.AncientsData.GetAncientLevel(AncientIds.Khrysos);
            Hero hero;
            var currentGold = khrysosLevel > 0 && this.gameData.Heroes.TryGetValue((int)khrysosLevel + 1, out hero)
                ? (BigDecimal)hero.BaseCost
                : 0;
            currentGold += 10
                * MonsterLife(startingZone)
                * MonsterGoldFactor(startingZone)
                * (currentTime >= timeUntilIdle ? (1 + this.GetFactor(AncientIds.Libertas, 0.01, 0.01)) : 1)
                * (1 + this.GetFactor(AncientIds.Mammon, 0.05, 0.05));

            var currentSouls = 0d;

            var levelingPlan = new LevelingPlan(
                this.gameData,
                this.savedGame.HeroesData,
                this.GetFactor(AncientIds.Argaiv, 0.02, 0.01),
                this.GetFactor(AncientIds.Dogcog, 0.02, 0.01));
            SimulateResult best = null;
            var bestRatio = 0d;

            var skills = new Skills(this.savedGame.AncientsData, this.itemLevels, this.GetFactor);
            for (var level = startingZone; level <= 4400; level++)
            {
                if (this.playStyle == PlayStyle.Active)
                {
                    skills.ActivateAll(currentTime, true);
                }

                var clickstormClicksPerSecond = skills.Value(0, currentTime);
                var powersurgeMultiplier = skills.Value(1, currentTime) + 1;
                var luckyStrikesMultiplier = Math.Min(1, skills.Value(2, currentTime) + 0.09);
                var metalDetectorMultiplier = skills.Value(3, currentTime) + 1;
                var goldenClicksMultiplier = skills.Value(4, currentTime) * (1 + this.GetFactor(AncientIds.Pluto, 0.3, 0.15)) / 100;
                var darkRitualMultiplier = skills.Value(5, currentTime);
                var superClicksMultiplier = skills.Value(6, currentTime) + 1;

                var clicksPerSecond = 0d;

                // Always click if Clickstorm is going (for Midas Start).
                if (this.playStyle == PlayStyle.Active || clickstormClicksPerSecond > 0)
                {
                    clicksPerSecond = Math.Min(40, clickstormClicksPerSecond + ActiveClicksPerSecond);
                }

                var isBoss = level % 5 == 0;
                var numMobs = isBoss ? 1 : 10 - (int)this.savedGame.AncientsData.GetAncientLevel(AncientIds.Kumawakamaru);
                var monsterLife = MonsterLife(level);
                if (isBoss)
                {
                    monsterLife *= 10 * (1 - this.GetFactor(AncientIds.Bubos, 0.02, 0.01));
                }

                var planDamage = levelingPlan.NextStep(currentGold);
                var planDamageBase = planDamage.Damage;
                if (planDamageBase < 10)
                {
                    planDamageBase = 10;
                }

                var dpsThisLevel = planDamageBase * damageFactor * darkRitualMultiplier * powersurgeMultiplier;

                var goldMobThisLevel = monsterLife
                    * MonsterGoldFactor(level)
                    * (1 + this.GetFactor(AncientIds.Mammon, 0.05, 0.05))
                    * (1 + this.GetFactor(AncientIds.Fortuna, 0.0225, 0.0225))
                    * planDamage.GoldMultiplier
                    * metalDetectorMultiplier;

                if (!isBoss)
                {
                    goldMobThisLevel *= 1 + (0.01 * (1 + this.GetFactor(AncientIds.Dora, 0.2, 0.1)) * ((10 * (1 + this.GetFactor(AncientIds.Mimzee, 0.5, 0.25))) - 1));
                }

                // Currently Idling?
                if ((clicksPerSecond == 0) && (currentTime >= timeUntilIdle))
                {
                    dpsThisLevel *= 1 + this.GetFactor(AncientIds.Siyalatas, 0.01, 0.1);
                    goldMobThisLevel *= 1 + this.GetFactor(AncientIds.Libertas, 0.01, 0.01);

                    // 5 minutes and clicking combo is gone.
                    if (currentTime > (timeUntilIdle + 240))
                    {
                        clicks = 0;
                    }
                }
                else
                {
                    dpsThisLevel *= 1 + (clicks * 0.0001 * this.savedGame.AncientsData.GetAncientLevel(AncientIds.Juggernaut));
                }

                var timeStart = currentTime;
                if (clicksPerSecond == 0)
                {
                    var numMobsLeft = numMobs;
                    while (numMobsLeft > 0)
                    {
                        currentTime += (double)(monsterLife / dpsThisLevel);
                        numMobsLeft -= 1;
                        if (numMobsLeft > 0)
                        {
                            currentTime += 0.5;
                        }

                        // Advance to the next frame (1/30th of a sec).
                        currentTime = Math.Floor((currentTime * 30) + 1) / 30;
                    }
                }
                else
                {
                    var currentLife = monsterLife;
                    var numMobsLeft = numMobs;
                    var clickDamage = dpsThisLevel * 0.035 * (1 + this.GetFactor(AncientIds.Fragsworth, 0.2, 0.1)) * superClicksMultiplier;
                    var clickDamagePlusDps = clickDamage + (dpsThisLevel / clicksPerSecond);
                    var criticalDamage = clickDamage * 18 * (1 + this.GetFactor(AncientIds.Bhaal, 0.15, 0.01));
                    var clicksLast = clicks;

                    // Calculates the next click at currentTime or greater.
                    var timeNextClick = Math.Ceiling(currentTime * clicksPerSecond) / clicksPerSecond;

                    while (numMobsLeft > 0)
                    {
                        var timeUntilClick = timeNextClick - currentTime;
                        var timeToKill = (double)(currentLife / dpsThisLevel);
                        if (timeUntilClick >= timeToKill)
                        {
                            // Die before next click
                            currentLife = 0;
                            currentTime += timeToKill;
                        }
                        else
                        {
                            // Short circuit if we're on the first mob of the first boss level and haven't killed it after an hour
                            if (level == startingZone
                                && numMobsLeft == numMobs
                                && currentTime > 3600)
                            {
                                return new SimulateResult(level, currentTime, 0, 0);
                            }

                            // Remove HP representing time before next click
                            currentLife -= timeUntilClick * dpsThisLevel;
                            currentTime = timeNextClick;

                            // Now, let's remove multiple normal clicks worth in one shot (includes the dps)
                            var clicksRegular = Math.Min((int)Math.Floor((double)(currentLife / clickDamagePlusDps)), (int)Math.Ceiling((1 - accumulatorCrit) / luckyStrikesMultiplier));
                            if (clicksRegular > 0)
                            {
                                currentTime += clicksRegular / clicksPerSecond;
                                clicks += clicksRegular;
                                currentLife -= clicksRegular * clickDamagePlusDps;
                                accumulatorCrit += luckyStrikesMultiplier * clicksRegular;
                            }

                            timeUntilIdle = currentTime + 60;
                            if (currentLife > 0)
                            {
                                // Okay, let's process one more click, but this time it might be a crit.
                                if (accumulatorCrit < 1)
                                {
                                    currentLife -= clickDamage;
                                }
                                else
                                {
                                    accumulatorCrit -= 1;
                                    currentLife -= criticalDamage;
                                }

                                clicks += 1;
                                accumulatorCrit += luckyStrikesMultiplier;

                                timeNextClick = currentTime + (1 / clicksPerSecond);
                            }
                        }

                        if (currentLife <= 0)
                        {
                            currentLife = monsterLife;
                            numMobsLeft -= 1;
                            if (numMobsLeft > 0)
                            {
                                currentTime += 0.5;

                                // Update Juggernaut effect.
                                if (clicks > clicksLast)
                                {
                                    var juggernautMultiplier = 0.0001 * this.savedGame.AncientsData.GetAncientLevel(AncientIds.Juggernaut);
                                    var mult = (1 + (clicks * juggernautMultiplier)) / (1 + (clicksLast * juggernautMultiplier));
                                    clickDamage *= mult;
                                    clickDamagePlusDps *= mult;
                                    criticalDamage *= mult;
                                    dpsThisLevel *= mult;
                                    clicksLast = clicks;
                                }
                            }

                            // Advance to the next frame (1/30th of a sec).
                            currentTime = Math.Floor((currentTime * 30) + 1) / 30;
                            timeNextClick = Math.Ceiling(currentTime * clicksPerSecond) / clicksPerSecond;   // Calculates the next click at currentTime or greater.
                        }
                    }
                }

                if (goldenClicksMultiplier > 0)
                {
                    var goldGC = Math.Floor((Math.Min(currentTime, skills.End(4)) - timeStart) * clicksPerSecond) * goldMobThisLevel * goldenClicksMultiplier;
                    if (isBoss)
                    {
                        goldGC *= 0.1;
                    }

                    currentGold += goldGC;
                }

                currentGold += numMobs * goldMobThisLevel;

                if (isBoss)
                {
                    currentSouls += this.HeroSoulRewards(level);

                    // Never stop before 140.
                    if (level < 140)
                    {
                        continue;
                    }

                    // If we already have a best, then stop if we can't kill the boss.
                    if (best != null && (currentTime - timeStart) > (30 + this.GetFactor(AncientIds.Chronos, 5, 5)))
                    {
                        break;
                    }

                    var realSouls = currentSouls + levelingPlan.GetSoulsFromHeroLevels(this.gameData, currentGold);

                    var ratio = realSouls / currentTime;
                    if (ratio > bestRatio)
                    {
                        bestRatio = ratio;
                    }

                    if (best == null || (ratio >= bestRatio))
                    {
                        best = new SimulateResult(level, currentTime, realSouls, realSouls / currentTime);
                    }
                    else
                    {
                        // ratio down to 50% (minimum of 1000 levels), it won't get better, stop simulation
                        if ((ratio < (best.Ratio * 0.5)) && (level > (startingZone + 1000)))
                        {
                            break;
                        }
                    }
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

        private static BigDecimal MonsterLife(int level)
        {
            return 10
                * (Math.Pow(1.6, Math.Min(level, 140) - 1) + Math.Min(level, 140) - 1)
                * BigDecimal.Pow(1.15, Math.Max(level - 140, 0));
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

        private double HeroSoulRewards(int level)
        {
            if (level == 100)
            {
                return 1;
            }
            else if (level > 100)
            {
                var solomonMultiplier = 1 + this.GetFactor(AncientIds.Solomon, 0.01, 0.01);
                var souls = Math.Floor(Math.Pow((((level - 100) / 5d) + 4d) / 5d, 1.3d) * solomonMultiplier);
                if (level % 100 != 0)
                {
                    souls *= 0.25 + this.GetFactor(AncientIds.Atman, 0.01, 0.01);
                }

                return souls;
            }

            return 0;
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
            long itemValue;
            if (ancientId == AncientIds.Solomon)
            {
                itemValue = SolomonValue(itemLevel);
            }
            else if (ancientId == AncientIds.Libertas)
            {
                itemValue = IdleValue(itemLevel);
            }
            else
            {
                itemValue = itemLevel;
            }

            return (ancientMultiplier * ancientValue) + (itemMultiplier * itemValue);
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

        private class Skills
        {
            private static Skill[] baseSkills =
            {
                // Clickstorm
                new Skill
                {
                    AncientId = AncientIds.Chawedo,
                    Value = 10,
                    Cooldown = 600
                },

                // Powersurge
                new Skill
                {
                    AncientId = AncientIds.Berserker,
                    Value = 1,
                    Cooldown = 600
                },

                // Lucky Strikes
                new Skill
                {
                    AncientId = AncientIds.Sniperino,
                    Value = 0.5,
                    Cooldown = 1800
                },

                // Metal Detector
                new Skill
                {
                    AncientId = AncientIds.Energon,
                    Value = 1,
                    Cooldown = 1800
                },

                // Golden Clicks
                new Skill
                {
                    AncientId = AncientIds.Kleptos,
                    Value = 0.01,
                    Cooldown = 3600
                },

                // The Dark Ritual
                new Skill
                {
                    AncientId = null,
                    Value = 0.05,
                    Cooldown = 28800
                },

                // Super Clicks
                new Skill
                {
                    AncientId = AncientIds.Hecatoncheir,
                    Value = 2,
                    Cooldown = 3600
                },

                // Energize
                new Skill
                {
                    AncientId = null,
                    Value = 1,
                    Cooldown = 3600
                },

                // Reload
                new Skill
                {
                    AncientId = null,
                    Value = 0,
                    Cooldown = 3600
                }
            };

            private static int[] prioritizedSkills = new[] { 0, 1, 2, 3, 4, 6, 7, 5, 8 };

            private readonly double factorVaagur;

            private readonly SkillUse[] skills;

            private int? lastSkillId;

            private int energize;

            private double darkRitualMultiplier;

            public Skills(
                object ancientLevels,
                IDictionary<int, int> itemLevels,
                Func<int, double, double, double> getFactor)
            {
                this.factorVaagur = 1 - getFactor(AncientIds.Vaagur, 0.05, 0);
                this.skills = new SkillUse[baseSkills.Length];
                this.energize = 1;
                this.darkRitualMultiplier = 1;

                for (var i = 0; i < baseSkills.Length; i++)
                {
                    var duration = 30d;
                    var ancientId = baseSkills[i].AncientId;
                    if (ancientId.HasValue)
                    {
                        duration += getFactor(ancientId.Value, 2, 1);
                    }

                    this.skills[i] = new SkillUse
                    {
                        Start = 0,
                        End = 0,
                        Available = 0,
                        Energized = 1,
                        Duration = duration
                    };
                }
            }

            public double Value(int skillId, double time)
            {
                if (skillId == 5)
                {
                    return this.darkRitualMultiplier;
                }

                var skill = this.skills[skillId];
                if (time >= skill.Start && time < skill.End)
                {
                    return skill.Energized * baseSkills[skillId].Value;
                }

                return 0;
            }

            // Valid on skills 1 to 5 and 7
            public double End(int skillId)
            {
                return this.skills[skillId].End;
            }

            public void Activate(int skillId, double time, bool onlyIfInactive)
            {
                var skill = this.skills[skillId];

                // Skill not refreshed yet or still active?
                if ((skill.Available > time) || (onlyIfInactive && skill.End > time))
                {
                    return;
                }

                switch (skillId)
                {
                    // The Dark Ritual
                    case 5:
                    {
                        this.darkRitualMultiplier *= 1 + (0.05 * this.energize);
                        break;
                    }

                    // Energize
                    case 7:
                    {
                        this.energize = 2;
                        return;
                    }

                    // Reload
                    case 8:
                    {
                        if (!this.lastSkillId.HasValue)
                        {
                            // Don't waste Reload
                            return;
                        }

                        this.skills[this.lastSkillId.Value].Available -= 3600;
                        break;
                    }

                    default:
                    {
                        skill.Start = time;
                        skill.End = time + skill.Duration;
                        skill.Energized = this.energize;
                        break;
                    }
                }

                skill.Available = time + (baseSkills[skillId].Cooldown * this.factorVaagur);
                this.energize = 1;
                this.lastSkillId = skillId;
            }

            public void ActivateAll(double time, bool onlyIfInactive)
            {
                foreach (var skillId in prioritizedSkills)
                {
                    this.Activate(skillId, time, onlyIfInactive);
                }
            }

            private sealed class Skill
            {
                public int? AncientId { get; set; }

                public double Value { get; set; }

                public int Cooldown { get; set; }
            }

            private sealed class SkillUse
            {
                public double Start { get; set; }

                public double End { get; set; }

                public double Available { get; set; }

                public int Energized { get; set; }

                public double Duration { get; set; }
            }
        }
    }
}