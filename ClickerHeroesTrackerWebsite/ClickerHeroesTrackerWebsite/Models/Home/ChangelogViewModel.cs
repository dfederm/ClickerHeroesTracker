// <copyright file="ChangelogViewModel.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Home
{
    public class ChangelogViewModel
    {
        public ChangelogViewModel(bool isFull)
        {
            this.IsFull = isFull;
        }

        public bool IsFull { get; }
    }
}