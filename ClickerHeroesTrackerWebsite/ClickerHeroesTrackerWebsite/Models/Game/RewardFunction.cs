// <copyright file="RewardFunction.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Game
{
    /// <summary>
    /// Represents a reward function from the game
    /// </summary>
    public enum RewardFunction
    {
        /// <summary>
        /// An unknown function. This should only happen when a new patch releases
        /// and this model has not updated it.
        /// </summary>
        Unknown,

        /// <summary>
        /// Adds to dps.
        /// </summary>
        AddDps,

        /// <summary>
        /// Adds to click damage
        /// </summary>
        AddClickDamage,

        /// <summary>
        /// Adds to hero soul percent
        /// </summary>
        AddSouls,
    }
}