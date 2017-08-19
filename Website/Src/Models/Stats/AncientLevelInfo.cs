﻿// <copyright file="AncientLevelInfo.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Stats
{
    using System.Numerics;

    /// <summary>
    /// A class that represents the effective level for an ancient.
    /// </summary>
    public sealed class AncientLevelInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AncientLevelInfo"/> class.
        /// </summary>
        /// <param name="ancientLevel">The base ancient level</param>
        /// <param name="itemLevel">The ancient levels added by items</param>
        public AncientLevelInfo(BigInteger ancientLevel, BigInteger itemLevel)
        {
            this.AncientLevel = ancientLevel;
            this.ItemLevel = itemLevel;
        }

        /// <summary>
        /// Gets the base ancient level
        /// </summary>
        public BigInteger AncientLevel { get; }

        /// <summary>
        /// Gets the ancient levels added by items
        /// </summary>
        public BigInteger ItemLevel { get; }

        /// <summary>
        /// Gets the effective level of the ancient.
        /// </summary>
        public BigInteger EffectiveLevel => this.AncientLevel + this.ItemLevel;
    }
}