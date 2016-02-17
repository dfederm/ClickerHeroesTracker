// <copyright file="UpgradeFunction.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Game
{
    /// <summary>
    /// Represents the effect an upgrade has on the game
    /// </summary>
    public enum UpgradeFunction
    {
        /// <summary>
        /// An unknown function. This should only happen when a new patch releases
        /// and this model has not updated it.
        /// </summary>
        Unknown,

        /// <summary>
        /// Upgrades click damage percent.
        /// </summary>
        UpgradeClickPercent,

        /// <summary>
        /// Unloicks a skill
        /// </summary>
        UpgradeGetSkill,

        /// <summary>
        /// Upgrades a hero's dps
        /// </summary>
        UpgradeHeroPercent,

        /// <summary>
        /// Upgrades all heroes' dps
        /// </summary>
        UpgradeEveryonePercent,

        /// <summary>
        /// Upgrades critical click damage
        /// </summary>
        UpgradeCriticalDamage,

        /// <summary>
        /// Upgrades critical click chance
        /// </summary>
        UpgradeCriticalChance,

        /// <summary>
        /// Upgrades the gold drop amount percentage
        /// </summary>
        UpgradeGoldFoundPercent,

        /// <summary>
        /// Upgrade click damage by a percent of the user's dps.
        /// </summary>
        UpgradeClickDpsPercent,

        /// <summary>
        /// Ascension
        /// </summary>
        FinalUpgrade,

        /// <summary>
        /// Unused
        /// </summary>
        FinalUpgrade2,
    }
}