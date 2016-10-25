// <copyright file="MockAuthenticationOwinMiddleware.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Services.Authentication
{
    using System;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Security.Claims;
    using System.Text.Encodings.Web;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using static MockAuthenticationOwinMiddleware;

    /// <summary>
    /// Middleware which mocks the authentication with the identity data from the request.
    /// </summary>
    /// <remarks>
    /// This should never be used in production, as it allows complete bypass of authentication.
    /// </remarks>
    internal sealed class MockAuthenticationOwinMiddleware : AuthenticationMiddleware<MockAuthenticationOptions>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MockAuthenticationOwinMiddleware"/> class.
        /// </summary>
        public MockAuthenticationOwinMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, UrlEncoder encoder)
            : base(next, new OptionsWrapper<MockAuthenticationOptions>(new MockAuthenticationOptions()), loggerFactory, encoder)
        {
        }

        /// <inheritdoc />
        protected override AuthenticationHandler<MockAuthenticationOptions> CreateHandler()
        {
            return new MockAuthenticationHandler();
        }

        internal sealed class MockAuthenticationOptions : AuthenticationOptions
        {
            public MockAuthenticationOptions()
            {
                this.ClaimsIssuer = "Mock";
                this.AuthenticationScheme = "Mock";
                this.AutomaticAuthenticate = true;
            }
        }

        private sealed class MockAuthenticationHandler : AuthenticationHandler<MockAuthenticationOptions>
        {
            private static readonly char[] AuthorizationTokenDelimeter = new[] { ':' };

            private static readonly char[] RoleDelimeter = new[] { ',' };

            protected override Task<AuthenticateResult> HandleAuthenticateAsync()
            {
                AuthenticationHeaderValue authorization;

                // Supported format: "Authorization: <Scheme==AuthenticationType> <Parameter>"
                var authorizationHeaderRaw = this.Request.Headers["Authorization"];
                if (!AuthenticationHeaderValue.TryParse(authorizationHeaderRaw, out authorization)
                    || authorization == null
                    || string.IsNullOrWhiteSpace(authorization.Scheme)
                    || !authorization.Scheme.Equals(this.Options.AuthenticationScheme, StringComparison.OrdinalIgnoreCase)
                    || string.IsNullOrWhiteSpace(authorization.Parameter))
                {
                    return Task.FromResult(AuthenticateResult.Fail("Unexpected AuthenticationScheme"));
                }

                // Supportered parameter format: "<UserId>:<UserName>:[<Role1>,<Role2>,...]"
                var parts = authorization.Parameter.Split(AuthorizationTokenDelimeter);
                if (parts.Length != 3)
                {
                    return Task.FromResult(AuthenticateResult.Fail("Unexpected Format"));
                }

                // Create the mock identity
                var identity = new ClaimsIdentity(
                    new Claim[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, parts[0]),
                        new Claim(ClaimTypes.Name, parts[1]),
                        new Claim(ClaimTypes.Email, parts[1] + "@test.com"),
                    }
                    .Concat(parts[2].Split(RoleDelimeter, StringSplitOptions.RemoveEmptyEntries).Select(role => new Claim(ClaimTypes.Role, role))),
                    new IdentityOptions().Cookies.ApplicationCookieAuthenticationScheme);

                var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), null, this.Options.AuthenticationScheme);
                return Task.FromResult(AuthenticateResult.Success(ticket));
            }
        }
    }
}