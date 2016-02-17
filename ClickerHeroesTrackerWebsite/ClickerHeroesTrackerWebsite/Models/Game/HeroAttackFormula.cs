// <copyright file="HeroAttackFormula.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Game
{
    /// <summary>
    /// Represents a hero attack formula from the game
    /// </summary>
    public enum HeroAttackFormula
    {
        /// <summary>
        /// An unknown formula. This should only happen when a new patch releases
        /// and this model has not updated it.
        /// </summary>
        Unknown,

        /// <summary>
        /// The hero attack damage formula.
        /// </summary>
        HeroAttackFormula1,
    }
}