// <copyright file="Scaling.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Game
{
    /// <summary>
    /// Represents a scaling function from the game
    /// </summary>
    public enum Scaling
    {
        /// <summary>
        /// An unknown value. This should only happen when a new patch releases
        /// and this model has not updated it.
        /// </summary>
        Unknown,

        /// <summary>
        /// Linear
        /// </summary>
        Linear,

        /// <summary>
        /// Cubic
        /// </summary>
        Cubic,
    }
}