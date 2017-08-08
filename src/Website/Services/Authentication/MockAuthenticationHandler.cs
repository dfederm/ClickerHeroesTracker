// <copyright file="MockAuthenticationHandler.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Services.Authentication
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Security.Claims;
    using System.Text.Encodings.Web;
    using System.Threading.Tasks;
    using AspNet.Security.OpenIdConnect.Primitives;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Authentication handler which mocks the authentication with the identity data from the request.
    /// </summary>
    internal sealed class MockAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private static readonly char[] AuthorizationTokenDelimeter = new[] { ':' };

        private static readonly char[] RoleDelimeter = new[] { ',' };

        // In order to avoid mock auth creating a security hole, we only allow a certain set of user ids to be used.
        // Ensure none of these are real user ids.
        private static readonly HashSet<string> AllowedUserIds = new HashSet<string>(new[]
        {
            "00000000-0000-0000-0000-000000000000",
        });

        public MockAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Supported format: "Authorization: <Scheme==AuthenticationType> <Parameter>"
            var authorizationHeaderRaw = this.Request.Headers["Authorization"];
            if (!AuthenticationHeaderValue.TryParse(authorizationHeaderRaw, out var authorization)
                || authorization == null
                || string.IsNullOrWhiteSpace(authorization.Scheme)
                || !authorization.Scheme.Equals(this.Scheme.Name, StringComparison.OrdinalIgnoreCase)
                || string.IsNullOrWhiteSpace(authorization.Parameter))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            // Supportered parameter format: "<UserId>:<UserName>:[<Role1>,<Role2>,...]"
            var parts = authorization.Parameter.Split(AuthorizationTokenDelimeter);
            if (parts.Length != 3)
            {
                return Task.FromResult(AuthenticateResult.Fail("Unexpected Format"));
            }

            var userId = parts[0];
            var userName = parts[1];
            var roles = parts[2];

            if (!AllowedUserIds.Contains(userId))
            {
                return Task.FromResult(AuthenticateResult.Fail("User id not in the set of allowed user ids for this scheme"));
            }

            // Create the mock identity
            var identity = new ClaimsIdentity(
                new Claim[]
                {
                    new Claim(OpenIdConnectConstants.Claims.Subject, userId),
                    new Claim(OpenIdConnectConstants.Claims.Name, userName),
                    new Claim(ClaimTypes.Email, userName + "@test.com"),
                }
                .Concat(roles.Split(RoleDelimeter, StringSplitOptions.RemoveEmptyEntries).Select(role => new Claim(OpenIdConnectConstants.Claims.Role, role))),
                IdentityConstants.ApplicationScheme,
                OpenIdConnectConstants.Claims.Name,
                OpenIdConnectConstants.Claims.Role);

            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), null, this.Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}