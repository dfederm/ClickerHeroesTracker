// <copyright file="MiscellaneousStatsModel.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Stats
{
    using System;
    using System.Linq;
    using ClickerHeroesTrackerWebsite.Models.Game;
    using ClickerHeroesTrackerWebsite.Models.SaveData;

    public class MiscellaneousStatsModel
    {
        public MiscellaneousStatsModel(SavedGame savedGame)
        {
            this.HeroSoulsSpent = savedGame.AncientsData.Ancients.Values.Aggregate(0d, (count, ancientData) => count + ancientData.SpentHeroSouls);
            this.HeroSoulsSacrificed = savedGame.HeroSoulsSacrificed;
            this.TitanDamage = savedGame.TitanDamage;
            this.TotalAncientSouls = savedGame.AncientSoulsTotal;
            var currentPhandoryssLevel = savedGame.OutsidersData != null && savedGame.OutsidersData.Outsiders != null
                ? savedGame.OutsidersData.Outsiders.GetOutsiderLevel(OutsiderIds.Phandoryss)
                : 0;
            this.TranscendentPower = savedGame.Transcendent
                ? (50 - (49 * Math.Pow(Math.E, -this.TotalAncientSouls / 10000)) + (currentPhandoryssLevel * 0.05)) / 100
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
            this.MaxTranscendentPrimalReward = Math.Floor(this.HeroSoulsSacrificed * (0.05 + (0.005 * currentBorbLevel)));

            double solomonMultiplier;
            var currentPonyboyLevel = savedGame.OutsidersData != null && savedGame.OutsidersData.Outsiders != null
                ? savedGame.OutsidersData.Outsiders.GetOutsiderLevel(OutsiderIds.Ponyboy)
                : 0;
            var currentSolomonLevel = savedGame.AncientsData.GetAncientLevel(AncientIds.Solomon);
            if (currentSolomonLevel < 21)
            {
                solomonMultiplier = 1 + ((1 + currentPonyboyLevel) * (0.05 * currentSolomonLevel));
            }
            else if (currentSolomonLevel < 41)
            {
                solomonMultiplier = 1 + ((1 + currentPonyboyLevel) * (1 + (0.04 * (currentSolomonLevel - 20))));
            }
            else if (currentSolomonLevel < 61)
            {
                solomonMultiplier = 1 + ((1 + currentPonyboyLevel) * (1.8 + (0.03 * (currentSolomonLevel - 40))));
            }
            else if (currentSolomonLevel < 81)
            {
                solomonMultiplier = 1 + ((1 + currentPonyboyLevel) * (2.4 + (0.02 * (currentSolomonLevel - 60))));
            }
            else
            {
                solomonMultiplier = 1 + ((1 + currentPonyboyLevel) * (2.8 + (0.01 * (currentSolomonLevel - 80))));
            }

            if (savedGame.Transcendent)
            {
                var bossNumber = Math.Ceiling(Math.Log(this.MaxTranscendentPrimalReward / (20 * solomonMultiplier)) / Math.Log(1 + this.TranscendentPower));

                // If the boss number is <= 0, that basically means the player is always at the cap. Since zone 100 always gives 0 from TP, the cap is technically 105.
                this.BossLevelToTranscendentPrimalCap = bossNumber > 0
                    ? (bossNumber * 5) + 100
                    : 105;
            }

            this.HeroSouls = savedGame.HeroSouls;
            this.PendingSouls = savedGame.PendingSouls;
        }

        public double HeroSoulsSpent { get; }

        public double HeroSoulsSacrificed { get; }

        public double TitanDamage { get; }

        public double TotalAncientSouls { get; }

        public double TranscendentPower { get; }

        public double Rubies { get; }

        public double HighestZoneThisTranscension { get; }

        public double HighestZoneLifetime { get; }

        public double AscensionsThisTranscension { get; }

        public double AscensionsLifetime { get; }

        public double MaxTranscendentPrimalReward { get; }

        public double BossLevelToTranscendentPrimalCap { get; }

        public double HeroSouls { get; }

        public double PendingSouls { get; }
    }
}