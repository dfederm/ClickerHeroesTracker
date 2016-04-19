// <copyright file="ManageLoginsViewModel.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.ViewModels.Manage
{
    using System.Collections.Generic;
    using Microsoft.AspNet.Http.Authentication;
    using Microsoft.AspNet.Identity;

    /// <summary>
    /// Model for the manage logins view
    /// </summary>
    public class ManageLoginsViewModel
    {
        /// <summary>
        /// Gets or sets a collection of the user's current external login providers.
        /// </summary>
        public IList<UserLoginInfo> CurrentLogins { get; set; }

        /// <summary>
        /// Gets or sets a collection of the remaining external login providers the user can choose.
        /// </summary>
        public IList<AuthenticationDescription> OtherLogins { get; set; }
    }
}