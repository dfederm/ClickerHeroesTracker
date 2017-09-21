// <copyright file="AuthenticationController.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Controllers.Api
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AspNet.Security.OpenIdConnect.Extensions;
    using AspNet.Security.OpenIdConnect.Primitives;
    using AspNet.Security.OpenIdConnect.Server;
    using ClickerHeroesTrackerWebsite.Models;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using OpenIddict.Core;
    using Website.Services.Authentication;

    [Route("api/auth")]
    public class AuthenticationController : Controller
    {
        private static readonly string[] AllowedRefreshTokenScopes = new[]
        {
            OpenIdConnectConstants.Scopes.OpenId,
            OpenIdConnectConstants.Scopes.Email,
            OpenIdConnectConstants.Scopes.Profile,
            OpenIdConnectConstants.Scopes.OfflineAccess,
            OpenIddictConstants.Scopes.Roles,
        };

        private readonly IOptions<IdentityOptions> identityOptions;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IAssertionGrantHandlerProvider assertionGrantHandlerProvider;

        public AuthenticationController(
            IOptions<IdentityOptions> identityOptions,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IAssertionGrantHandlerProvider assertionGrantHandlerProvider)
        {
            this.identityOptions = identityOptions;
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.assertionGrantHandlerProvider = assertionGrantHandlerProvider;
        }

        [HttpPost("token")]
        [Produces("application/json")]
        public async Task<IActionResult> Exchange(OpenIdConnectRequest request)
        {
            if (request.IsPasswordGrantType())
            {
                var user = await this.userManager.FindByNameAsync(request.Username);
                if (user == null)
                {
                    return this.BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.InvalidGrant,
                        ErrorDescription = "Incorrect username or password.",
                    });
                }

                // Validate the username/password parameters and ensure the account is not locked out.
                var result = await this.signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
                if (!result.Succeeded)
                {
                    return this.BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.InvalidGrant,
                        ErrorDescription = "The username/password couple is invalid.",
                    });
                }

                return await this.SignInAsync(request, user);
            }

            if (request.IsRefreshTokenGrantType())
            {
                // Retrieve the claims principal stored in the refresh token.
                var result = await this.HttpContext.AuthenticateAsync(OpenIdConnectServerDefaults.AuthenticationScheme);

                // Retrieve the user profile corresponding to the refresh token.
                var user = await this.signInManager.ValidateSecurityStampAsync(result.Principal);
                if (user == null)
                {
                    return this.BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.InvalidGrant,
                        ErrorDescription = "The refresh token is no longer valid.",
                    });
                }

                // Ensure the user is still allowed to sign in.
                if (!await this.signInManager.CanSignInAsync(user))
                {
                    return this.BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.InvalidGrant,
                        ErrorDescription = "The user is no longer allowed to sign in.",
                    });
                }

                // Reuse the properties stored in the refresh token, including the scopes originally granted.
                return await this.SignInAsync(request, user, result.Properties);
            }

            var assertionGrantHandler = this.assertionGrantHandlerProvider.GetHandler(request.GrantType);
            if (assertionGrantHandler != null)
            {
                // Reject the request if the "assertion" parameter is missing.
                if (string.IsNullOrEmpty(request.Assertion))
                {
                    return this.BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.InvalidRequest,
                        ErrorDescription = "The mandatory 'assertion' parameter was missing.",
                    });
                }

                var result = await assertionGrantHandler.ValidateAsync(request.Assertion);
                if (!result.IsSuccessful)
                {
                    return this.BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.InvalidGrant,
                        ErrorDescription = result.Error,
                    });
                }

                // Find the user associated with this external log in
                var user = await this.userManager.FindByLoginAsync(assertionGrantHandler.Name, result.ExternalUserId);
                if (user == null)
                {
                    // If the user does not have an account, then ask the user to create an account.
                    return this.BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.AccountSelectionRequired,
                    });
                }

                // Ensure the user is still allowed to sign in.
                if (!await this.signInManager.CanSignInAsync(user))
                {
                    return this.BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.InvalidGrant,
                        ErrorDescription = "The user is no longer allowed to sign in.",
                    });
                }

                return await this.SignInAsync(request, user);
            }

            return this.BadRequest(new OpenIdConnectResponse
            {
                Error = OpenIdConnectConstants.Errors.UnsupportedGrantType,
                ErrorDescription = "The specified grant type is not supported.",
            });
        }

        private async Task<IActionResult> SignInAsync(OpenIdConnectRequest request, ApplicationUser user, AuthenticationProperties properties = null)
        {
            // Create a new ClaimsPrincipal containing the claims that
            // will be used to create an id_token, a token or a code.
            var principal = await this.signInManager.CreateUserPrincipalAsync(user);

            // Create a new authentication ticket holding the user identity.
            var ticket = new AuthenticationTicket(principal, properties, OpenIdConnectServerDefaults.AuthenticationScheme);

            if (!request.IsRefreshTokenGrantType())
            {
                // Set the list of scopes granted to the client application.
                // Note: the offline_access scope must be granted
                // to allow OpenIddict to return a refresh token.
                ticket.SetScopes(AllowedRefreshTokenScopes.Intersect(request.GetScopes()));
            }

            ticket.SetResources("resource-server");

            // Note: by default, claims are NOT automatically included in the access and identity tokens.
            // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
            // whether they should be included in access tokens, in identity tokens or in both.
            foreach (var claim in ticket.Principal.Claims)
            {
                // Never include the security stamp in the access and identity tokens, as it's a secret value.
                if (claim.Type == this.identityOptions.Value.ClaimsIdentity.SecurityStampClaimType)
                {
                    continue;
                }

                var destinations = new List<string>
                {
                    OpenIdConnectConstants.Destinations.AccessToken,
                };

                // Only add the iterated claim to the id_token if the corresponding scope was granted to the client application.
                // The other claims will only be added to the access_token, which is encrypted when using the default format.
                if ((claim.Type == OpenIdConnectConstants.Claims.Name && ticket.HasScope(OpenIdConnectConstants.Scopes.Profile))
                    || (claim.Type == OpenIdConnectConstants.Claims.Email && ticket.HasScope(OpenIdConnectConstants.Scopes.Email))
                    || (claim.Type == OpenIdConnectConstants.Claims.Role && ticket.HasScope(OpenIddictConstants.Claims.Roles)))
                {
                    destinations.Add(OpenIdConnectConstants.Destinations.IdentityToken);
                }

                claim.SetDestinations(destinations);
            }

            return this.SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
        }
    }
}
