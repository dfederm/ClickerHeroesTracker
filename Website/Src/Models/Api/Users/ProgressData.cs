// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System.Collections.Generic;

namespace ClickerHeroesTrackerWebsite.Models.Api.Users
{
    /// <summary>
    /// An aggregation of progress data for a user.
    /// </summary>
    public class ProgressData
    {
        public IDictionary<string, string> TitanDamageData { get; set; }

        public IDictionary<string, string> SoulsSpentData { get; set; }

        public IDictionary<string, string> HeroSoulsSacrificedData { get; set; }

        public IDictionary<string, string> TotalAncientSoulsData { get; set; }

        public IDictionary<string, string> TranscendentPowerData { get; set; }

        public IDictionary<string, string> RubiesData { get; set; }

        public IDictionary<string, string> HighestZoneThisTranscensionData { get; set; }

        public IDictionary<string, string> HighestZoneLifetimeData { get; set; }

        public IDictionary<string, string> AscensionsThisTranscensionData { get; set; }

        public IDictionary<string, string> AscensionsLifetimeData { get; set; }

        public IDictionary<string, IDictionary<string, string>> AncientLevelData { get; set; }

        public IDictionary<string, IDictionary<string, string>> OutsiderLevelData { get; set; }
    }
}