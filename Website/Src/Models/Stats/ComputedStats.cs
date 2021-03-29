// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System;
using System.Linq;
using System.Numerics;
using ClickerHeroesTrackerWebsite.Models.SaveData;
using ClickerHeroesTrackerWebsite.Utility;
using Newtonsoft.Json.Linq;

namespace ClickerHeroesTrackerWebsite.Models.Stats
{
    public class ComputedStats
    {
        public ComputedStats(SavedGame savedGame)
        {
            bool transcendent = savedGame.Object.Value<bool>("transcendent");

            HeroSoulsSpent = savedGame.Object["ancients"]["ancients"]
                .OfType<JProperty>()
                .Aggregate(BigInteger.Zero, (count, ancientData) => count + ancientData.Value.Value<string>("spentHeroSouls").ToBigInteger())
                .ToTransportableString();

            HeroSoulsSacrificed = savedGame.Object.Value<string>("heroSoulsSacrificed");
            TitanDamage = savedGame.Object.Value<string>("titanDamage");
            TotalAncientSouls = savedGame.Object.Value<long>("ancientSoulsTotal");
            Rubies = savedGame.Object.Value<long>("rubies");
            HighestZoneThisTranscension = savedGame.Object.Value<long>("highestFinishedZonePersist");
            HighestZoneLifetime = Math.Max(savedGame.Object.Value<long>("pretranscendentHighestFinishedZone"), Math.Max(savedGame.Object.Value<long>("transcendentHighestFinishedZone"), HighestZoneThisTranscension));

            AscensionsThisTranscension = savedGame.Object.Value<long>(transcendent ? "numAscensionsThisTranscension" : "numWorldResets");
            AscensionsLifetime = savedGame.Object.Value<long>("numWorldResets");
            TranscendentPower = transcendent
                ? (2 + (23 * (1 - Math.Pow(Math.E, -0.0003 * TotalAncientSouls)))) / 100
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