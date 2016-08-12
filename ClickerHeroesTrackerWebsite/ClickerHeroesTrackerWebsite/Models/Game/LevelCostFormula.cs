// <copyright file="LevelCostFormula.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Game
{
    /// <summary>
    /// Represents a scaling function from the game
    /// </summary>
    public enum LevelCostFormula
    {
        /// <summary>
        /// One
        /// </summary>
        One,

        /// <summary>
        /// Linear, 1x
        /// </summary>
        Linear,

        /// <summary>
        /// Polynomial, x^1.5
        /// </summary>
        Polynomial1_5,

        /// <summary>
        /// Quadratic
        /// </summary>
        /// <remarks>
        /// This is only used for non-transcendant ancients.
        /// </remarks>
        Quadratic,

        /// <summary>
        /// Exponential, 2^x
        /// </summary>
        Exponential,
    }
}