// <copyright file="User.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Api.Uploads
{
    /// <summary>
    /// The user details returned in a specific upload details
    /// </summary>
    public sealed class User
    {
        /// <summary>
        /// Gets or sets the user's id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the user's name.
        /// </summary>
        public string Name { get; set; }
    }
}