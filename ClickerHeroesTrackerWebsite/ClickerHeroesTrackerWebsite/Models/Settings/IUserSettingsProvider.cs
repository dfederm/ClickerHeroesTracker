// <copyright file="IUserSettingsProvider.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Settings
{
    public interface IUserSettingsProvider
    {
        IUserSettings Get(string userId);

        void FlushChanges();
    }
}
