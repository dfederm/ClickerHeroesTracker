// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System.Collections.Generic;

namespace ClickerHeroesTrackerWebsite.Configuration
{
    /// <summary>
    /// Provides information about the currently running environment.
    /// </summary>
    public interface IBuildInfoProvider
    {
        /// <summary>
        /// Gets the changlist of the last change that is part of the running build.
        /// </summary>
        string Changelist { get; }

        /// <summary>
        /// Gets the build url which produced the binaries for the running build.
        /// </summary>
        string BuildUrl { get; }

        /// <summary>
        /// Gets a mapping of each Webclient file to its version.
        /// </summary>
        IDictionary<string, string> Webclient { get; }
    }
}
