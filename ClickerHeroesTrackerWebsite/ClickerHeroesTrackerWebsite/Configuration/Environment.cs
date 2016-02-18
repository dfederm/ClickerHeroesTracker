// <copyright file="Environment.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Configuration
{
    /// <summary>
    /// The environment the service is running on
    /// </summary>
    public enum Environment
    {
        /// <summary>
        /// A developer's machine
        /// </summary>
        Devmachine,

        /// <summary>
        /// The environement used for buddy builds
        /// </summary>
        Buddy,

        /// <summary>
        /// The staging (pre-prod) environment
        /// </summary>
        Staging,

        /// <summary>
        /// THe production environment.
        /// </summary>
        Production
    }
}