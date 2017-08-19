// <copyright file="MiscellaneousStatsModel.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Stats
{
    using System;
    using System.Linq;
    using System.Numerics;
    using ClickerHeroesTrackerWebsite.Models.Game;
    using ClickerHeroesTrackerWebsite.Models.SaveData;

    public class MiscellaneousStatsModel
    {
        public MiscellaneousStatsModel(SavedGame savedGame)
        {
            this.HeroSoulsSpent = savedGame.AncientsData.Ancients.Values.Aggregate(BigInteger.Zero, (count, ancientData) => count + ancientData.SpentHeroSouls);
            this.HeroSoulsSacrificed = savedGame.HeroSoulsSacrificed;
            this.TitanDamage = savedGame.TitanDamage;
            this.TotalAncientSouls = savedGame.AncientSoulsTotal;
            var currentPhandoryssLevel = savedGame.OutsidersData != null && savedGame.OutsidersData.Outsiders != null
                ? savedGame.OutsidersData.Outsiders.GetOutsiderLevel(OutsiderIds.Phandoryss)
                : 0;
            this.Rubies = savedGame.Rubies;
            this.HighestZoneThisTranscension = savedGame.HighestFinishedZonePersist;
            this.HighestZoneLifetime = Math.Max(savedGame.PretranscendentHighestFinishedZone, Math.Max(savedGame.TranscendentHighestFinishedZone, this.HighestZoneThisTranscension));
            this.AscensionsThisTranscension = savedGame.Transcendent
                ? savedGame.NumAscensionsThisTranscension
                : savedGame.NumWorldResets;
            this.AscensionsLifetime = savedGame.NumWorldResets;

            var currentBorbLevel = savedGame.OutsidersData != null && savedGame.OutsidersData.Outsiders != null
                ? savedGame.OutsidersData.Outsiders.GetOutsiderLevel(OutsiderIds.Borb)
                : 0;

            if (savedGame.Transcendent)
            {
                var tpFromAncientSouls = 50 - (49 * Math.Pow(Math.E, -this.TotalAncientSouls / 10000d));
                var tpFromPhandoryss = 50 * (1 - Math.Pow(Math.E, -currentPhandoryssLevel / 1000d));
                this.TranscendentPower = (tpFromAncientSouls + tpFromPhandoryss) / 100;

                // This is equivalent to: this.HeroSoulsSacrificed * (0.05 + (0.005 * currentBorbLevel))
                // but multiplied and then divided by 200 to defer the loss of precision division until the end.
                this.MaxTranscendentPrimalReward = (this.HeroSoulsSacrificed * new BigInteger(10 + currentBorbLevel)) / 200;

                var currentPonyboyLevel = (savedGame.OutsidersData?.Outsiders?.GetOutsiderLevel(OutsiderIds.Ponyboy)).GetValueOrDefault();

                double maxRewardLog;
                var currentSolomonLevel = savedGame.AncientsData.GetAncientLevel(AncientIds.Solomon);
                if (currentSolomonLevel > long.MaxValue)
                {
                    // Take a slight loss of precision for large numbers
                    var solomonMultiplier = 1 + ((1 + currentPonyboyLevel) * (currentSolomonLevel / 100));
                    maxRewardLog = BigInteger.Log(this.MaxTranscendentPrimalReward / (20 * solomonMultiplier));
                }
                else
                {
                    var currentSolomonLevelDouble = (double)currentSolomonLevel;

                    double solomonMultiplier;
                    if (currentSolomonLevel < 21)
                    {
                        solomonMultiplier = 1 + ((1 + currentPonyboyLevel) * (0.05 * currentSolomonLevelDouble));
                    }
                    else if (currentSolomonLevel < 41)
                    {
                        solomonMultiplier = 1 + ((1 + currentPonyboyLevel) * (1 + (0.04 * (currentSolomonLevelDouble - 20))));
                    }
                    else if (currentSolomonLevel < 61)
                    {
                        solomonMultiplier = 1 + ((1 + currentPonyboyLevel) * (1.8 + (0.03 * (currentSolomonLevelDouble - 40))));
                    }
                    else if (currentSolomonLevel < 81)
                    {
                        solomonMultiplier = 1 + ((1 + currentPonyboyLevel) * (2.4 + (0.02 * (currentSolomonLevelDouble - 60))));
                    }
                    else
                    {
                        solomonMultiplier = 1 + ((1 + currentPonyboyLevel) * (2.8 + (0.01 * (currentSolomonLevelDouble - 80))));
                    }

                    // If the numbers are sufficiently low enough, just cast and use exact values
                    if (this.MaxTranscendentPrimalReward < long.MaxValue)
                    {
                        maxRewardLog = Math.Log((double)this.MaxTranscendentPrimalReward / (20 * solomonMultiplier));
                    }
                    else
                    {
                        // If the numbers are sufficiently large enough, we can take a loss of precision
                        maxRewardLog = BigInteger.Log(DivideWithPrecisionLoss(this.MaxTranscendentPrimalReward, 20 * solomonMultiplier));
                    }
                }

                var bossNumber = (long)Math.Ceiling(maxRewardLog / Math.Log(1 + this.TranscendentPower));

                // If the boss number is <= 0, that basically means the player is always at the cap. Since zone 100 always gives 0 from TP, the cap is technically 105.
                this.BossLevelToTranscendentPrimalCap = bossNumber > 0
                    ? ((bossNumber * 5) + 100)
                    : 105;
            }
            else
            {
                this.TranscendentPower = 0;
            }

            this.HeroSouls = savedGame.HeroSouls;
            this.PendingSouls = savedGame.PendingSouls;
        }

        public BigInteger HeroSoulsSpent { get; }

        public BigInteger HeroSoulsSacrificed { get; }

        public BigInteger TitanDamage { get; }

        public long TotalAncientSouls { get; }

        public double TranscendentPower { get; }

        public long Rubies { get; }

        public long HighestZoneThisTranscension { get; }

        public long HighestZoneLifetime { get; }

        public long AscensionsThisTranscension { get; }

        public long AscensionsLifetime { get; }

        public BigInteger MaxTranscendentPrimalReward { get; }

        public long BossLevelToTranscendentPrimalCap { get; }

        public BigInteger HeroSouls { get; set; }

        public BigInteger PendingSouls { get; }

        private static BigInteger DivideWithPrecisionLoss(BigInteger n, double divisor)
        {
            const long precision = long.MaxValue;
            n *= precision;
            n /= new BigInteger(divisor * precision);
            return n;
        }
    }
}