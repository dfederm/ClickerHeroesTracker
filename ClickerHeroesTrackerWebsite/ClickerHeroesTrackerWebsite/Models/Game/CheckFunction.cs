// <copyright file="CheckFunction.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Game
{
    /// <summary>
    /// Represents a function to check for conditions.
    /// </summary>
    public enum CheckFunction
    {
        /// <summary>
        /// An unknown function. This should only happen when a new patch releases
        /// and this model has not updated it.
        /// </summary>
        Unknown,

        /// <summary>
        /// Checks the highest zone ever.
        /// </summary>
        HighestFinishedZone,

        /// <summary>
        /// The total number of clicks
        /// </summary>
        TotalClicks,

        /// <summary>
        /// The total gold ever earned
        /// </summary>
        TotalGold,

        /// <summary>
        /// The gold held at one time
        /// </summary>
        Gold,

        /// <summary>
        /// The total number of monster kills
        /// </summary>
        TotalKills,

        /// <summary>
        /// The total number of boss kills
        /// </summary>
        TotalBossKills,

        /// <summary>
        /// The total number of purchased upgrades
        /// </summary>
        TotalUpgrades,

        /// <summary>
        /// The total number of hero levels
        /// </summary>
        TotalHeroLevels,

        /// <summary>
        /// The highest dps ever obtained.
        /// </summary>
        MaxDps,

        /// <summary>
        /// Most clicks performed in a second.
        /// </summary>
        MostClicksPerSecond,

        /// <summary>
        /// Most critical clicks performed in a second.
        /// </summary>
        MostCritsPerSecond,

        /// <summary>
        /// Total number of times ascended.
        /// </summary>
        NumWorldResets,

        /// <summary>
        /// Total number of chests killed.
        /// </summary>
        TreasureChestsKilled,

        /// <summary>
        /// Total number of relics recieved
        /// </summary>
        TotalRelicsReceived,

        /// <summary>
        /// Total number of mercenaries buried
        /// </summary>
        TotalMercenariesBuried,

        /// <summary>
        /// Total number of mercenaries revived
        /// </summary>
        TotalMercenariesRevived,

        /// <summary>
        /// Total number of gold quests completed
        /// </summary>
        GoldQuestsCompleted,

        /// <summary>
        /// Total number of relic quests completed
        /// </summary>
        RelicQuestsCompleted,

        /// <summary>
        /// Total number of ruby quests completed
        /// </summary>
        RubyQuestsCompleted,

        /// <summary>
        /// Total number of skill quests completed
        /// </summary>
        SkillQuestsCompleted,

        /// <summary>
        /// Total number of soul quests completed
        /// </summary>
        HeroSoulQuestsCompleted,

        /// <summary>
        /// Total number of 5 minute quests completed
        /// </summary>
        Total5MinuteQuests,

        /// <summary>
        /// Leeroy Jenkins dying
        /// </summary>
        LeeroyJenkinsBuried,

        /// <summary>
        /// Highest mercenary level obtained
        /// </summary>
        HighestMercenaryLevelEver,

        /// <summary>
        /// Rarest mercenary obtained
        /// </summary>
        RarestMercenaryEver,

        /// <summary>
        /// Total number of mercenaries hired.
        /// </summary>
        MercenaryCount,
    }
}