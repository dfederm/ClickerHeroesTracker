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
            this.Rubies = savedGame.Rubies;
            this.HighestZoneThisTranscension = savedGame.HighestFinishedZonePersist;
            this.HighestZoneLifetime = Math.Max(savedGame.PretranscendentHighestFinishedZone, Math.Max(savedGame.TranscendentHighestFinishedZone, this.HighestZoneThisTranscension));
            this.AscensionsThisTranscension = savedGame.Transcendent
                ? savedGame.NumAscensionsThisTranscension
                : savedGame.NumWorldResets;
            this.AscensionsLifetime = savedGame.NumWorldResets;
            this.TranscendentPower = savedGame.Transcendent
                ? (2 + (23 * (1 - Math.Pow(Math.E, -0.0003 * this.TotalAncientSouls)))) / 100
                : 0;
            this.HeroSouls = savedGame.HeroSouls;
            this.PendingSouls = savedGame.PendingSouls;
            this.Autoclickers = savedGame.Autoclickers;
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

        public BigInteger HeroSouls { get; set; }

        public BigInteger PendingSouls { get; }

        public long Autoclickers { get; }

        private static BigInteger DivideWithPrecisionLoss(BigInteger n, double divisor)
        {
            const long precision = long.MaxValue;
            n *= precision;
            n /= new BigInteger(divisor * precision);
            return n;
        }
    }
}