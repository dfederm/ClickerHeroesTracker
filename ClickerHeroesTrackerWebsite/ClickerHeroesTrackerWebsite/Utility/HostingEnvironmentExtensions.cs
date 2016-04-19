// <copyright file="HostingEnvironmentExtensions.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Utility
{
    using System;
    using Microsoft.AspNet.Hosting;

    /// <summary>
    /// Extension methods for <see cref="IHostingEnvironment"/>.
    /// </summary>
    internal static class HostingEnvironmentExtensions
    {
        /// <summary>
        /// Checks if the current hosting environment name is "Buddy".
        /// </summary>
        /// <param name="hostingEnvironment">An instance of <see cref="IHostingEnvironment"/>.</param>
        /// <returns>True if the environment name is "Development", otherwise false.</returns>
        public static bool IsBuddy(this IHostingEnvironment hostingEnvironment)
        {
            if (hostingEnvironment == null)
            {
                throw new ArgumentNullException(nameof(hostingEnvironment));
            }

            return hostingEnvironment.IsEnvironment("Buddy");
        }
    }
}
