// <copyright file="IEnvironmentProvider.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Configuration
{
    /// <summary>
    /// Provides information about the currently running environment
    /// </summary>
    public interface IEnvironmentProvider
    {
        /// <summary>
        /// Gets the environment name
        /// </summary>
        string Environment { get; }

        /// <summary>
        /// Gets the changlist of the last change that is part of the running build.
        /// </summary>
        int Changelist { get; }

        /// <summary>
        /// Gets the build id of the running service.
        /// </summary>
        string BuildId { get; }
    }
}
