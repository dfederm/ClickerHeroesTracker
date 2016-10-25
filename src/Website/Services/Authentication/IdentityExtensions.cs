// <copyright file="IdentityExtensions.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Services.Authentication
{
    using System.Security.Claims;
    using System.Threading.Tasks;
    using ClickerHeroesTrackerWebsite.Models;
    using Microsoft.AspNetCore.Identity;

    /// <summary>
    /// Identity-based extensions.
    /// </summary>
    public static class IdentityExtensions
    {
        /// <summary>
        /// Gets the user's email address
        /// </summary>
        /// <returns>The user's email address</returns>
        public static async Task<string> GetEmailAsync(this ClaimsPrincipal principal, UserManager<ApplicationUser> userManager)
        {
            // Try to get the claim first
            var claim = principal.FindFirst(ClaimTypes.Email);
            if (claim != null)
            {
                return claim.Value;
            }

            // Fall back to looking it up in the DB
            return await userManager.GetEmailAsync(await userManager.GetUserAsync(principal));
        }
    }
}