// <copyright file="HeroCostFormula.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Game
{
    /// <summary>
    /// Represents a hero cost formula from the game
    /// </summary>
    public enum HeroCostFormula
    {
        /// <summary>
        /// An unknown function. This should only happen when a new patch releases
        /// and this model has not updated it.
        /// </summary>
        Unknown,

        /// <summary>
        /// Gold per second formula.
        /// </summary>
        HeroCostFormula1,
    }
}