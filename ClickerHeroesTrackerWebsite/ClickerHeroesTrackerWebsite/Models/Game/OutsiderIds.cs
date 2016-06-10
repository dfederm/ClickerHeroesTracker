// <copyright file="OutsiderIds.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Game
{
    using System.Collections.Generic;
    using ClickerHeroesTrackerWebsite.Models.Api.Stats;

    /// <summary>
    /// Constants that represent various outsider ids to help with clean code. Be sure to keep this in sync with the game data.
    /// </summary>
    public static class OutsiderIds
    {
        /// <summary>
        /// Xyliqil
        /// </summary>
        public const int Xyliqil = 1;

        /// <summary>
        /// Chor'gorloth
        /// </summary>
        public const int Chorgorloth = 2;

        /// <summary>
        /// Phandoryss
        /// </summary>
        public const int Phandoryss = 3;

        /// <summary>
        /// Borb
        /// </summary>
        public const int Borb = 4;

        /// <summary>
        /// Ponyboy
        /// </summary>
        public const int Ponyboy = 5;

        private static readonly Dictionary<int, StatType> OutsiderStatTypeMap = new Dictionary<int, StatType>
        {
            { OutsiderIds.Borb, StatType.OutsiderBorb },
            { OutsiderIds.Chorgorloth, StatType.OutsiderChorgorloth },
            { OutsiderIds.Phandoryss, StatType.OutsiderPhandoryss },
            { OutsiderIds.Ponyboy, StatType.OutsiderPonyboy },
            { OutsiderIds.Xyliqil, StatType.OutsiderXyliqil },
        };

        public static StatType GetOusiderStatType(int outsiderId)
        {
            StatType statType;
            return OutsiderStatTypeMap.TryGetValue(outsiderId, out statType)
                ? statType
                : StatType.Unknown;
        }
    }
}