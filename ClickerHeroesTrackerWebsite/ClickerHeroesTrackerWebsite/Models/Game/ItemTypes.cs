// <copyright file="ItemTypes.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Game
{
    using System.Collections.Generic;

    public static class ItemTypes
    {
        private static Dictionary<int, Ancient> itemTypeMap = new Dictionary<int, Ancient>
        {
            { 1, Ancient.Siyalatas },
            { 2, Ancient.Fragsworth },
            { 3, Ancient.Chronos },
            { 4, Ancient.Chawedo },
            { 5, Ancient.Revolc },
            { 6, Ancient.Iris },
            { 7, Ancient.Argaiv },
            { 8, Ancient.Energon },
            { 9, Ancient.Kleptos },
            { 10, Ancient.Sniperino },
            { 11, Ancient.Berserker },
            { 12, Ancient.Hecatoncheir },
            { 13, Ancient.Bubos },
            { 14, Ancient.Morgulis },
            { 15, Ancient.Bhaal },
            { 16, Ancient.Dora },
            { 17, Ancient.Atman },
            { 18, Ancient.Fortuna },
            { 19, Ancient.Dogcog },
            { 20, Ancient.Pluto },
            { 21, Ancient.Mimzee },
            { 22, Ancient.Mammon },
            { 24, Ancient.Libertas },
            { 25, Ancient.Solomon }
        };

        public static Ancient GetAncient(int itemType)
        {
            Ancient ancient;
            return itemTypeMap.TryGetValue(itemType, out ancient)
                ? ancient
                : null;
        }
    }
}