// <copyright file="HeroClickDamageFormula.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Game
{
    /// <summary>
    /// Represents a click damage formula in the game
    /// </summary>
    public enum HeroClickDamageFormula
    {
        /// <summary>
        /// An unknown function. This should only happen when a new patch releases
        /// and this model has not updated it.
        /// </summary>
        Unknown,

        /// <summary>
        /// The click damage formula
        /// </summary>
        HeroGoldPerSecondFormula,

        /// <summary>
        /// The click damage formula
        /// </summary>
        HeroClickDamageFormula1,
    }
}