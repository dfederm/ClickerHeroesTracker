// <copyright file="MiscellaneousStatsModel.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Stats
{
    using System;
    using System.Linq;
    using ClickerHeroesTrackerWebsite.Models.Game;
    using ClickerHeroesTrackerWebsite.Models.SaveData;

    /// <summary>
    /// The model for the miscellaneous table
    /// </summary>
    public class MiscellaneousStatsModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SuggestedAncientLevelsModel"/> class.
        /// </summary>
        public MiscellaneousStatsModel(
            GameData gameData,
            SavedGame savedGame)
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
            this.HighestZoneLifetime = Math.Max(savedGame.TranscendentHighestFinishedZone, this.HighestZoneThisTranscension);
            this.AscensionsThisTranscension = savedGame.NumAscensionsThisTranscension != 0
                ? savedGame.NumAscensionsThisTranscension
                : savedGame.NumWorldResets;
            this.AscensionsLifetime = savedGame.NumWorldResets;

            var currentBorbLevel = savedGame.OutsidersData != null && savedGame.OutsidersData.Outsiders != null
                ? savedGame.OutsidersData.Outsiders.GetOutsiderLevel(OutsiderIds.Borb)
                : 0;
            this.MaxTranscendentPrimalReward = this.HeroSoulsSacrificed * (0.05 + (0.005 * currentBorbLevel));

            double solomonMultiplier;
            var currentPonyboyLevel = savedGame.OutsidersData != null && savedGame.OutsidersData.Outsiders != null
                ? savedGame.OutsidersData.Outsiders.GetOutsiderLevel(OutsiderIds.Ponyboy)
                : 0;
            var currentSolomonLevel = savedGame.AncientsData.GetAncientLevel(AncientIds.Solomon);
            if (currentSolomonLevel < 21)
            {
                solomonMultiplier = 1 + (1 + currentPonyboyLevel) * (0.05 * currentSolomonLevel);
            }
            else if (currentSolomonLevel < 41)
            {
                solomonMultiplier = 1 + (1 + currentPonyboyLevel) * (1 + (0.04 * (currentSolomonLevel - 20)));
            }
            else if (currentSolomonLevel < 61)
            {
                solomonMultiplier = 1 + (1 + currentPonyboyLevel) * (1.8 + (0.03 * (currentSolomonLevel - 40)));
            }
            else if (currentSolomonLevel < 81)
            {
                solomonMultiplier = 1 + (1 + currentPonyboyLevel) * (2.4 + (0.02 * (currentSolomonLevel - 60)));
            }
            else
            {
                solomonMultiplier = 1 + (1 + currentPonyboyLevel) * (2.8 + (0.01 * (currentSolomonLevel - 80)));
            }

            var bossNumber = Math.Ceiling(Math.Log(this.MaxTranscendentPrimalReward / (20 * solomonMultiplier)) / Math.Log(1 + this.TranscendentPower));
            this.BossLevelToTranscendentPrimalCap = (bossNumber * 5) + 100;
        }

        /// <summary>
        /// Gets the hero souls spent
        /// </summary>
        public double HeroSoulsSpent { get; }

        /// <summary>
        /// Gets the hero souls earned for the user's lifetime
        /// </summary>
        public double HeroSoulsSacrificed { get; }

        /// <summary>
        /// Gets the titan damage
        /// </summary>
        public double TitanDamage { get; }

        /// <summary>
        /// Gets the total ancient souls
        /// </summary>
        public double TotalAncientSouls { get; }

        /// <summary>
        /// Gets the transcendent power
        /// </summary>
        public double TranscendentPower { get; }

        /// <summary>
        /// Gets the rubies the user currently has
        /// </summary>
        public double Rubies { get; }

        /// <summary>
        /// Gets the user's highest zone reached this transcension
        /// </summary>
        public double HighestZoneThisTranscension { get; }

        /// <summary>
        /// Gets the user's highest zone ever reached
        /// </summary>
        public double HighestZoneLifetime { get; }

        /// <summary>
        /// Gets the number of ascensions this transcension
        /// </summary>
        public double AscensionsThisTranscension { get; }

        /// <summary>
        /// Gets the number of ascensions ever
        /// </summary>
        public double AscensionsLifetime { get; }

        /// <summary>
        /// Gets the max transcendent primal reward.
        /// </summary>
        public double MaxTranscendentPrimalReward { get; }

        /// <summary>
        /// Gets the boss level at which the TP primal cap is reached.
        /// </summary>
        public double BossLevelToTranscendentPrimalCap { get; }
    }
}