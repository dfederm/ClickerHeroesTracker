// <copyright file="IDatabaseCommandFactory.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Database
{
    using System.Collections.Generic;
    using System.Data;

    /// <summary>
    /// A factory for creating <see cref="IDatabaseCommand"/>s.
    /// </summary>
    public interface IDatabaseCommandFactory
    {
        /// <summary>
        /// Creates an <see cref="IDatabaseCommand"/>.
        /// </summary>
        /// <returns>An <see cref="IDatabaseCommand"/></returns>
        IDatabaseCommand Create();
    }
}
