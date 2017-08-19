// <copyright file="RemoveLoginViewModel.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.ViewModels.Manage
{
    /// <summary>
    /// Model for the remove logins view
    /// </summary>
    public class RemoveLoginViewModel
    {
        /// <summary>
        /// Gets or sets the external login provider name.
        /// </summary>
        public string LoginProvider { get; set; }

        /// <summary>
        /// Gets or sets the external login provider key.
        /// </summary>
        public string ProviderKey { get; set; }
    }
}
