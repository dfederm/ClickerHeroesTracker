// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

namespace ClickerHeroesTrackerWebsite.Services.Database
{
    /// <summary>
    /// A factory for creating <see cref="IDatabaseCommand"/>s.
    /// </summary>
    public interface IDatabaseCommandFactory
    {
        /// <summary>
        /// Creates an <see cref="IDatabaseCommand"/>.
        /// </summary>
        /// <returns>An <see cref="IDatabaseCommand"/>.</returns>
        IDatabaseCommand Create();
    }
}
