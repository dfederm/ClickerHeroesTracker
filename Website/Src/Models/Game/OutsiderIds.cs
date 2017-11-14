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
        public const int Xyliqil = 1;
        public const int Chorgorloth = 2;
        public const int Phandoryss = 3;
        ////public const int Borb = 4; // This version of Borb was deprecated
        public const int Ponyboy = 5;
        public const int Borb = 6;
        public const int Rhageist = 7;
        public const int KAriqua = 8;
        public const int Orphalas = 9;
        public const int SenAkhan = 10;

        private static readonly Dictionary<int, StatType> OutsiderStatTypeMap = new Dictionary<int, StatType>
        {
            { OutsiderIds.Borb, StatType.OutsiderBorb },
            { OutsiderIds.Chorgorloth, StatType.OutsiderChorgorloth },
            { OutsiderIds.KAriqua, StatType.OutsiderKAriqua },
            { OutsiderIds.Orphalas, StatType.OutsiderOrphalas },
            { OutsiderIds.Phandoryss, StatType.OutsiderPhandoryss },
            { OutsiderIds.Ponyboy, StatType.OutsiderPonyboy },
            { OutsiderIds.Rhageist, StatType.OutsiderRhageist },
            { OutsiderIds.SenAkhan, StatType.OutsiderSenAkhan },
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