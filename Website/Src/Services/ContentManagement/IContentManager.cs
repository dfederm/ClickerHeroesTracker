// <copyright file="IContentManager.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Services.ContentManagement
{
    using System.Collections.Generic;

    public interface IContentManager
    {
        IEnumerable<string> Scripts { get; }

        IEnumerable<string> RawScripts { get; }

        void RegisterScript(string name);

        void RegisterRawScript(string content);
    }
}
