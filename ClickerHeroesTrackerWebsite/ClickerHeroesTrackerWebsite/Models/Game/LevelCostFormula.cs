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
        /// An unknown value. This should only happen when a new patch releases
        /// and this model has not updated it.
        /// </summary>
        Unknown,

        /// <summary>
        /// One
        /// </summary>
        One,

        /// <summary>
        /// Linear, .33x
        /// </summary>
        LinearEzPz,

        /// <summary>
        /// Linear, 1x
        /// </summary>
        Linear1,

        /// <summary>
        /// Linear, 10x
        /// </summary>
        Linear10,

        /// <summary>
        /// Quadratic
        /// </summary>
        Quadratic,

        /// <summary>
        /// Cubic
        /// </summary>
        Cubic,
    }
}