// <copyright file="AncientIds.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Game
{
    using System.Collections.Generic;
    using ClickerHeroesTrackerWebsite.Models.Api.Stats;

    /// <summary>
    /// Constants that represent various ancients ids to help with clean code. Be sure to keep this in sync with the game data.
    /// </summary>
    public static class AncientIds
    {
        /// <summary>
        /// Solomon
        /// </summary>
        public const int Solomon = 3;

        /// <summary>
        /// Libertas
        /// </summary>
        public const int Libertas = 4;

        /// <summary>
        /// Siyalatas
        /// </summary>
        public const int Siyalatas = 5;

        /// <summary>
        /// Khrysos
        /// </summary>
        public const int Khrysos = 6;

        /// <summary>
        /// Thusia
        /// </summary>
        public const int Thusia = 7;

        /// <summary>
        /// Mammon
        /// </summary>
        public const int Mammon = 8;

        /// <summary>
        /// Mimzee
        /// </summary>
        public const int Mimzee = 9;

        /// <summary>
        /// Pluto
        /// </summary>
        public const int Pluto = 10;

        /// <summary>
        /// Dogcog
        /// </summary>
        public const int Dogcog = 11;

        /// <summary>
        /// Fortuna
        /// </summary>
        public const int Fortuna = 12;

        /// <summary>
        /// Atman
        /// </summary>
        public const int Atman = 13;

        /// <summary>
        /// Dora
        /// </summary>
        public const int Dora = 14;

        /// <summary>
        /// Bhaal
        /// </summary>
        public const int Bhaal = 15;

        /// <summary>
        /// Morgulis
        /// </summary>
        public const int Morgulis = 16;

        /// <summary>
        /// Chronos
        /// </summary>
        public const int Chronos = 17;

        /// <summary>
        /// Bubos
        /// </summary>
        public const int Bubos = 18;

        /// <summary>
        /// Fragsworth
        /// </summary>
        public const int Fragsworth = 19;

        /// <summary>
        /// Vaagur
        /// </summary>
        public const int Vaagur = 20;

        /// <summary>
        /// Kumawakamaru
        /// </summary>
        public const int Kumawakamaru = 21;

        /// <summary>
        /// Chawedo
        /// </summary>
        public const int Chawedo = 22;

        /// <summary>
        /// Hecatoncheir
        /// </summary>
        public const int Hecatoncheir = 23;

        /// <summary>
        /// Berserker
        /// </summary>
        public const int Berserker = 24;

        /// <summary>
        /// Sniperino
        /// </summary>
        public const int Sniperino = 25;

        /// <summary>
        /// Kleptos
        /// </summary>
        public const int Kleptos = 26;

        /// <summary>
        /// Energon
        /// </summary>
        public const int Energon = 27;

        /// <summary>
        /// Argaiv
        /// </summary>
        public const int Argaiv = 28;

        /// <summary>
        /// Juggernaut
        /// </summary>
        public const int Juggernaut = 29;

        /// <summary>
        /// Iris
        /// </summary>
        public const int Iris = 30;

        /// <summary>
        /// Revolc
        /// </summary>
        public const int Revolc = 31;

        private static readonly Dictionary<int, StatType> AncientStatTypeMap = new Dictionary<int, StatType>
        {
            { AncientIds.Argaiv, StatType.AncientArgaiv },
            { AncientIds.Atman, StatType.AncientAtman },
            { AncientIds.Berserker, StatType.AncientBerserker },
            { AncientIds.Bhaal, StatType.AncientBhaal },
            { AncientIds.Bubos, StatType.AncientBubos },
            { AncientIds.Chawedo, StatType.AncientChawedo },
            { AncientIds.Chronos, StatType.AncientChronos },
            { AncientIds.Dogcog, StatType.AncientDogcog },
            { AncientIds.Dora, StatType.AncientDora },
            { AncientIds.Energon, StatType.AncientEnergon },
            { AncientIds.Fortuna, StatType.AncientFortuna },
            { AncientIds.Fragsworth, StatType.AncientFragsworth },
            { AncientIds.Hecatoncheir, StatType.AncientHecatoncheir },
            { AncientIds.Iris, StatType.AncientIris },
            { AncientIds.Juggernaut, StatType.AncientJuggernaut },
            { AncientIds.Khrysos, StatType.AncientKhrysos },
            { AncientIds.Kleptos, StatType.AncientKleptos },
            { AncientIds.Kumawakamaru, StatType.AncientKumawakamaru },
            { AncientIds.Libertas, StatType.AncientLibertas },
            { AncientIds.Mammon, StatType.AncientMammon },
            { AncientIds.Mimzee, StatType.AncientMimzee },
            { AncientIds.Morgulis, StatType.AncientMorgulis },
            { AncientIds.Pluto, StatType.AncientPluto },
            { AncientIds.Revolc, StatType.AncientRevolc },
            { AncientIds.Siyalatas, StatType.AncientSiyalatas },
            { AncientIds.Sniperino, StatType.AncientSniperino },
            { AncientIds.Solomon, StatType.AncientSolomon },
            { AncientIds.Thusia, StatType.AncientThusia },
            { AncientIds.Vaagur, StatType.AncientVaagur },
        };

        private static readonly Dictionary<int, StatType> ItemStatTypeMap = new Dictionary<int, StatType>
        {
            { AncientIds.Argaiv, StatType.ItemArgaiv },
            { AncientIds.Atman, StatType.ItemAtman },
            { AncientIds.Berserker, StatType.ItemBerserker },
            { AncientIds.Bhaal, StatType.ItemBhaal },
            { AncientIds.Bubos, StatType.ItemBubos },
            { AncientIds.Chawedo, StatType.ItemChawedo },
            { AncientIds.Chronos, StatType.ItemChronos },
            { AncientIds.Dogcog, StatType.ItemDogcog },
            { AncientIds.Dora, StatType.ItemDora },
            { AncientIds.Energon, StatType.ItemEnergon },
            { AncientIds.Fortuna, StatType.ItemFortuna },
            { AncientIds.Fragsworth, StatType.ItemFragsworth },
            { AncientIds.Hecatoncheir, StatType.ItemHecatoncheir },
            { AncientIds.Iris, StatType.ItemIris },
            { AncientIds.Juggernaut, StatType.ItemJuggernaut },
            { AncientIds.Khrysos, StatType.ItemKhrysos },
            { AncientIds.Kleptos, StatType.ItemKleptos },
            { AncientIds.Kumawakamaru, StatType.ItemKumawakamaru },
            { AncientIds.Libertas, StatType.ItemLibertas },
            { AncientIds.Mammon, StatType.ItemMammon },
            { AncientIds.Mimzee, StatType.ItemMimzee },
            { AncientIds.Morgulis, StatType.ItemMorgulis },
            { AncientIds.Pluto, StatType.ItemPluto },
            { AncientIds.Revolc, StatType.ItemRevolc },
            { AncientIds.Siyalatas, StatType.ItemSiyalatas },
            { AncientIds.Sniperino, StatType.ItemSniperino },
            { AncientIds.Solomon, StatType.ItemSolomon },
            { AncientIds.Thusia, StatType.ItemThusia },
            { AncientIds.Vaagur, StatType.ItemVaagur },
        };

        private static readonly Dictionary<int, StatType> SuggestedStatTypeMap = new Dictionary<int, StatType>
        {
            { AncientIds.Argaiv, StatType.SuggestedArgaiv },
            { AncientIds.Atman, StatType.SuggestedAtman },
            { AncientIds.Berserker, StatType.SuggestedBerserker },
            { AncientIds.Bhaal, StatType.SuggestedBhaal },
            { AncientIds.Bubos, StatType.SuggestedBubos },
            { AncientIds.Chawedo, StatType.SuggestedChawedo },
            { AncientIds.Chronos, StatType.SuggestedChronos },
            { AncientIds.Dogcog, StatType.SuggestedDogcog },
            { AncientIds.Dora, StatType.SuggestedDora },
            { AncientIds.Energon, StatType.SuggestedEnergon },
            { AncientIds.Fortuna, StatType.SuggestedFortuna },
            { AncientIds.Fragsworth, StatType.SuggestedFragsworth },
            { AncientIds.Hecatoncheir, StatType.SuggestedHecatoncheir },
            { AncientIds.Iris, StatType.SuggestedIris },
            { AncientIds.Juggernaut, StatType.SuggestedJuggernaut },
            { AncientIds.Khrysos, StatType.SuggestedKhrysos },
            { AncientIds.Kleptos, StatType.SuggestedKleptos },
            { AncientIds.Kumawakamaru, StatType.SuggestedKumawakamaru },
            { AncientIds.Libertas, StatType.SuggestedLibertas },
            { AncientIds.Mammon, StatType.SuggestedMammon },
            { AncientIds.Mimzee, StatType.SuggestedMimzee },
            { AncientIds.Morgulis, StatType.SuggestedMorgulis },
            { AncientIds.Pluto, StatType.SuggestedPluto },
            { AncientIds.Revolc, StatType.SuggestedRevolc },
            { AncientIds.Siyalatas, StatType.SuggestedSiyalatas },
            { AncientIds.Sniperino, StatType.SuggestedSniperino },
            { AncientIds.Solomon, StatType.SuggestedSolomon },
            { AncientIds.Thusia, StatType.SuggestedThusia },
            { AncientIds.Vaagur, StatType.SuggestedVaagur },
        };

        public static StatType GetAncientStatType(int ancientId)
        {
            StatType statType;
            return AncientStatTypeMap.TryGetValue(ancientId, out statType)
                ? statType
                : StatType.Unknown;
        }

        public static StatType GetItemStatType(int ancientId)
        {
            StatType statType;
            return ItemStatTypeMap.TryGetValue(ancientId, out statType)
                ? statType
                : StatType.Unknown;
        }

        public static StatType GetSuggestedStatType(int ancientId)
        {
            StatType statType;
            return SuggestedStatTypeMap.TryGetValue(ancientId, out statType)
                ? statType
                : StatType.Unknown;
        }
    }
}