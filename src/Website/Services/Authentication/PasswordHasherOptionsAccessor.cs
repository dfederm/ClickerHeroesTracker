// <copyright file="PasswordHasherOptionsAccessor.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Services.Authentication
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Represents the password hashing options
    /// </summary>
    public sealed class PasswordHasherOptionsAccessor : IOptions<PasswordHasherOptions>
    {
        /// <summary>
        /// Gets options which use the IdentityV2 compat mode since the database originally used identity v2 so the existing passwords used it for hashing.
        /// </summary>
        public PasswordHasherOptions Value { get; } = new PasswordHasherOptions { CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV2 };
    }
}
