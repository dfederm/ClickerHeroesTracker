// <copyright file="MockAuthenticationOwinMiddleware.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Authentication
{
    using System;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.Owin;
    using Microsoft.Owin.Security;
    using Microsoft.Owin.Security.Infrastructure;

    /// <summary>
    /// Middleware which mocks the authentication with the identity data from the request.
    /// </summary>
    /// <remarks>
    /// This should never be used in production, as it allows complete bypass of authentication.
    /// </remarks>
    public class MockAuthenticationOwinMiddleware : AuthenticationMiddleware<AuthenticationOptions>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MockAuthenticationOwinMiddleware"/> class.
        /// </summary>
        public MockAuthenticationOwinMiddleware(OwinMiddleware next)
            : base(next, new MockAuthenticationOptions())
        {
        }

        /// <inheritdoc />
        protected override AuthenticationHandler<AuthenticationOptions> CreateHandler()
        {
            return new MockAuthenticationHandler();
        }

        private sealed class MockAuthenticationOptions : AuthenticationOptions
        {
            public MockAuthenticationOptions()
                : base("Mock")
            {
            }
        }

        private sealed class MockAuthenticationHandler : AuthenticationHandler<AuthenticationOptions>
        {
            private static readonly AuthenticationTicket EmptyTicket = new AuthenticationTicket(null, null);

            private static readonly char[] AuthorizationTokenDelimeter = new[] { ':' };

            private static readonly char[] RoleDelimeter = new[] { ',' };

            protected override Task<AuthenticationTicket> AuthenticateCoreAsync()
            {
                AuthenticationHeaderValue authorization;

                // Supported format: "Authorization: <Scheme==AuthenticationType> <Parameter>"
                var authorizationHeaderRaw = this.Request.Headers["Authorization"];
                if (authorizationHeaderRaw == null
                    || !AuthenticationHeaderValue.TryParse(authorizationHeaderRaw, out authorization)
                    || authorization == null
                    || string.IsNullOrWhiteSpace(authorization.Scheme)
                    || !authorization.Scheme.Equals(this.Options.AuthenticationType, StringComparison.OrdinalIgnoreCase)
                    || string.IsNullOrWhiteSpace(authorization.Parameter))
                {
                    return Task.FromResult(EmptyTicket);
                }

                // Supportered parameter format: "<UserId>:<UserName>:[<Role1>,<Role2>,...]"
                var parts = authorization.Parameter.Split(AuthorizationTokenDelimeter);
                if (parts.Length != 3)
                {
                    return Task.FromResult(EmptyTicket);
                }

                // This claim is required by the AntiForgeryToken logic. See AntiForgeryConfig.UniqueClaimTypeIdentifier documentaiton for details.
                // We could also set AntiForgeryConfig.UniqueClaimTypeIdentifier, but this makes the code more portable by allowing it to use default logic.
                const string IdentityProviderClaimType = "http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider";

                // Create the mock identity
                var identity = new ClaimsIdentity(
                    new Claim[]
                    {
                        new Claim(IdentityProviderClaimType, this.Options.AuthenticationType, null, this.Options.AuthenticationType),
                        new Claim(ClaimTypes.NameIdentifier, parts[0], null, this.Options.AuthenticationType),
                        new Claim(ClaimTypes.Name, parts[1], null, this.Options.AuthenticationType),
                    }
                    .Concat(parts[2].Split(RoleDelimeter, StringSplitOptions.RemoveEmptyEntries).Select(role => new Claim(ClaimTypes.Role, role, null, this.Options.AuthenticationType))),
                    this.Options.AuthenticationType);

                var ticket = new AuthenticationTicket(identity, null);
                return Task.FromResult(ticket);
            }
        }
    }
}