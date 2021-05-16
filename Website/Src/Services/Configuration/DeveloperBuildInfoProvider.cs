// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System.Collections.Generic;

namespace ClickerHeroesTrackerWebsite.Configuration
{
    /// <summary>
    /// Implementation of <see cref="IBuildInfoProvider" /> for developer machines.
    /// </summary>
    public sealed class DeveloperBuildInfoProvider : IBuildInfoProvider
    {
        /// <inheritdoc/>
        public string Changelist => "0";

        /// <inheritdoc/>
        public string BuildUrl => "LOCAL";

        public IDictionary<string, string> Webclient { get; } = new Dictionary<string, string>();
    }
}