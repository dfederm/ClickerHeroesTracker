// <copyright file="ComputedStats.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Stats
{
    using System;
    using System.Linq;
    using System.Numerics;
    using ClickerHeroesTrackerWebsite.Models.SaveData;
    using ClickerHeroesTrackerWebsite.Utility;
    using Newtonsoft.Json.Linq;

    public class ComputedStats
    {
        public ComputedStats(SavedGame savedGame)
        {
            var transcendent = savedGame.Object.Value<bool>("transcendent");

            this.HeroSoulsSpent = savedGame.Object["ancients"]["ancients"]
                .OfType<JProperty>()
                .Aggregate(BigInteger.Zero, (count, ancientData) => count + ancientData.Value.Value<string>("spentHeroSouls").ToBigInteger())
                .ToTransportableString();

            this.HeroSoulsSacrificed = savedGame.Object.Value<string>("heroSoulsSacrificed");
            this.TitanDamage = savedGame.Object.Value<string>("titanDamage");
            this.TotalAncientSouls = savedGame.Object.Value<long>("ancientSoulsTotal");
            this.Rubies = savedGame.Object.Value<long>("rubies");
            this.HighestZoneThisTranscension = savedGame.Object.Value<long>("highestFinishedZonePersist");
            this.HighestZoneLifetime = Math.Max(savedGame.Object.Value<long>("pretranscendentHighestFinishedZone"), Math.Max(savedGame.Object.Value<long>("transcendentHighestFinishedZone"), this.HighestZoneThisTranscension));

            this.AscensionsThisTranscension = savedGame.Object.Value<long>(transcendent ? "numAscensionsThisTranscension" : "numWorldResets");
            this.AscensionsLifetime = savedGame.Object.Value<long>("numWorldResets");
            this.TranscendentPower = transcendent
                ? (2 + (23 * (1 - Math.Pow(Math.E, -0.0003 * this.TotalAncientSouls)))) / 100
                : 0;
        }

        public string HeroSoulsSpent { get; }

        public string HeroSoulsSacrificed { get; }

        public string TitanDamage { get; }

        public long TotalAncientSouls { get; }

        public double TranscendentPower { get; }

        public long Rubies { get; }

        public long HighestZoneThisTranscension { get; }

        public long HighestZoneLifetime { get; }

        public long AscensionsThisTranscension { get; }

        public long AscensionsLifetime { get; }
    }
}