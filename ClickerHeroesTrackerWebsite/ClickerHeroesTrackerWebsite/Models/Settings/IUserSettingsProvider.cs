// <copyright file="IUserSettingsProvider.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Settings
{
    /// <summary>
    /// A provider which can retrieve user settings based on the user id.
    /// </summary>
    public interface IUserSettingsProvider
    {
        /// <summary>
        /// Gets the user settings for a user
        /// </summary>
        /// <param name="userId">The user id for the user to fetch the settings for.</param>
        /// <returns>The user settings for the user</returns>
        IUserSettings Get(string userId);

        /// <summary>
        /// Commits any changes to the user settings to any backing store.
        /// </summary>
        void FlushChanges();
    }
}
