// <copyright file="IUserSettingsProvider.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Settings
{
    using Website.Models.Api.Users;

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
        UserSettings Get(string userId);

        /// <summary>
        /// Patch the user settings for a user. It will override settings but not clear any missing from the patch.
        /// </summary>
        /// <param name="userId">The user id for the user to patch the settings for.</param>
        /// <param name="userSettings">The settings to patch</param>
        void Patch(string userId, UserSettings userSettings);
    }
}
