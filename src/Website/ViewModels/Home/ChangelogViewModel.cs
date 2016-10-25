// <copyright file="ChangelogViewModel.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Home
{
    /// <summary>
    /// Model for the changelog view
    /// </summary>
    public class ChangelogViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChangelogViewModel"/> class.
        /// </summary>
        public ChangelogViewModel(bool isFull)
        {
            this.IsFull = isFull;
        }

        /// <summary>
        /// Gets a value indicating whether this is the full changelog UI or not.
        /// </summary>
        public bool IsFull { get; }
    }
}