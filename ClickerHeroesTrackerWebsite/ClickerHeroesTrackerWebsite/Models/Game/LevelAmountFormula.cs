// <copyright file="LevelAmountFormula.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Game
{
    /// <summary>
    /// Represents a scaling function from the game
    /// </summary>
    public enum LevelAmountFormula
    {
        /// <summary>
        /// An unknown value. This should only happen when a new patch releases
        /// and this model has not updated it.
        /// </summary>
        Unknown,

        /// <summary>
        /// Linear, 0.01x
        /// </summary>
        Linear01,

        /// <summary>
        /// Linear, 0.25x
        /// </summary>
        Linear0_25,

        /// <summary>
        /// Linear, 1x
        /// </summary>
        Linear1,

        /// <summary>
        /// Linear, 2x
        /// </summary>
        Linear2,

        /// <summary>
        /// Linear, 5x
        /// </summary>
        Linear5,

        /// <summary>
        /// Linear, 10x
        /// </summary>
        Linear10,

        /// <summary>
        /// Linear, 11x
        /// </summary>
        Linear11,

        /// <summary>
        /// Linear, 15x
        /// </summary>
        Linear15,

        /// <summary>
        /// Linear, 20x
        /// </summary>
        Linear20,

        /// <summary>
        /// Linear, 25x
        /// </summary>
        Linear25,

        /// <summary>
        /// Linear, 30x
        /// </summary>
        Linear30,

        /// <summary>
        /// Linear, 50x
        /// </summary>
        Linear50,

        /// <summary>
        /// Linear, 100x
        /// </summary>
        Linear100,

        /// <summary>
        /// Linear, special Liberatas and Siyalatas formula
        /// </summary>
        LibAndSiy,

        /// <summary>
        /// Linear, special Solomon formula
        /// </summary>
        SolomonRewards,

        /// <summary>
        /// Special amount for ascension upgrade
        /// </summary>
        AscendGoldAmount,
    }
}