// <copyright file="WebHostEnvironmentExtensions.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Utility
{
    using System;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// Extension methods for <see cref="IWebHostEnvironment"/>.
    /// </summary>
    internal static class WebHostEnvironmentExtensions
    {
        /// <summary>
        /// Checks if the current hosting environment name is "Buddy".
        /// </summary>
        /// <param name="webHostEnvironment">An instance of <see cref="IWebHostEnvironment"/>.</param>
        /// <returns>True if the environment name is "Development", otherwise false.</returns>
        public static bool IsBuddy(this IWebHostEnvironment webHostEnvironment)
        {
            if (webHostEnvironment == null)
            {
                throw new ArgumentNullException(nameof(webHostEnvironment));
            }

            return webHostEnvironment.IsEnvironment("Buddy");
        }
    }
}
