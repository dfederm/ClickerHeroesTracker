// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

namespace ClickerHeroesTrackerWebsite.Models
{
    /// <summary>
    /// The user's play style.
    /// </summary>
    public enum PlayStyle
    {
        /// <summary>
        /// The play style which involves being idle the whole time. No clicking or ability usage.
        /// </summary>
        Idle,

        /// <summary>
        /// The play style that involves being idle until the optimal level, then using cooldowns once before ascending.
        /// </summary>
        Hybrid,

        /// <summary>
        /// The play style that involves continuous clicking to build combos. No idling.
        /// </summary>
        Active,
    }
}