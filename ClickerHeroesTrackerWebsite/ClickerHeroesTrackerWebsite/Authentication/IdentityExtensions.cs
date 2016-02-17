// <copyright file="IdentityExtensions.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Authentication
{
    using System.Security.Claims;
    using System.Security.Principal;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.Owin;
    using Microsoft.Owin;

    /// <summary>
    /// Identity-based extensions.
    /// </summary>
    public static class IdentityExtensions
    {
        /// <summary>
        /// Gets the user's email address
        /// </summary>
        /// <returns>The user's email address</returns>
        public static string GetEmail(this IIdentity identity, IOwinContext owinContext)
        {
            // Try to get the claim first
            var claimsIdentity = identity as ClaimsIdentity;
            if (claimsIdentity != null)
            {
                var claim = claimsIdentity.FindFirst(ClaimTypes.Email);
                if (claim != null)
                {
                    return claim.Value;
                }
            }

            // Fall back to looking it up in the DB
            return owinContext
                .GetUserManager<ApplicationUserManager>()
                .GetEmail(identity.GetUserId());
        }
    }
}