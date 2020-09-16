// <copyright file="AuthenticationController.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using ClickerHeroesTrackerWebsite.Models;
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using OpenIddict.Abstractions;
    using OpenIddict.Server.AspNetCore;
    using Website.Services.Authentication;

    [Route("api/auth")]
    [ApiController]
    public class AuthenticationController : Controller
    {
        private static readonly string[] AllowedRefreshTokenScopes =
        {
            OpenIddictConstants.Scopes.OpenId,
            OpenIddictConstants.Scopes.Email,
            OpenIddictConstants.Scopes.Profile,
            OpenIddictConstants.Scopes.OfflineAccess,
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
        [Authorize] // Authorize + AllowAnonymous to basically force authentication to work without requiring it. There's probably a better way for this to work...
        [AllowAnonymous]
        public async Task<ActionResult> Exchange()
        {
            var request = this.HttpContext.GetOpenIddictServerRequest();

            if (request.IsPasswordGrantType())
            {
                // Allow the user to log in with their email address too.
                // We already check that usernames are only "word" chars (\w+), so this check is sufficient.
                var user = request.Username.Contains("@", StringComparison.Ordinal)
                    ? await this.userManager.FindByEmailAsync(request.Username)
                    : await this.userManager.FindByNameAsync(request.Username);
                if (user == null)
                {
                    var properties = new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The username/password couple is invalid.",
                    });

                    return this.Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                }

                // Validate the username/password parameters and ensure the account is not locked out.
                var result = await this.signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
                if (!result.Succeeded)
                {
                    var properties = new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The username/password couple is invalid.",
                    });

                    return this.Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                }

                return await this.SignInAsync(request, user);
            }

            if (request.IsRefreshTokenGrantType())
            {
                // Retrieve the claims principal stored in the refresh token.
                var result = await this.HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

                // Retrieve the user profile corresponding to the refresh token.
                var user = await this.signInManager.ValidateSecurityStampAsync(result.Principal);
                if (user == null)
                {
                    var properties = new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The refresh token is no longer valid.",
                    });

                    return this.Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                }

                // Ensure the user is still allowed to sign in.
                if (!await this.signInManager.CanSignInAsync(user))
                {
                    var properties = new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is no longer allowed to sign in.",
                    });

                    return this.Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                }

                // Reuse the properties stored in the refresh token, including the scopes originally granted.
                return await this.SignInAsync(request, user);
            }

            var assertionGrantHandler = this.assertionGrantHandlerProvider.GetHandler(request.GrantType);
            if (assertionGrantHandler != null)
            {
                // Reject the request if the "assertion" parameter is missing.
                if (string.IsNullOrEmpty(request.Assertion))
                {
                    var properties = new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidRequest,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The mandatory 'assertion' parameter was missing.",
                    });

                    return this.Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                }

                var validationResult = await assertionGrantHandler.ValidateAsync(request.Assertion);
                if (!validationResult.IsSuccessful)
                {
                    var properties = new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = validationResult.Error,
                    });

                    return this.Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                }

                // Find the user associated with this external log in
                var user = await this.userManager.FindByLoginAsync(assertionGrantHandler.Name, validationResult.ExternalUserId);
                if (user == null)
                {
                    if (!string.IsNullOrEmpty(request.Username))
                    {
                        // They provided a user name, so try to implicitly create an account for them
                        user = new ApplicationUser { UserName = request.Username, Email = validationResult.ExternalUserEmail };
                        var creationResult = await this.userManager.CreateAsync(user);
                        if (!creationResult.Succeeded)
                        {
                            var properties = new AuthenticationProperties(new Dictionary<string, string>
                            {
                                [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = string.Join(" ", creationResult.Errors.Select(error => error.Description)),
                            });

                            return this.Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                        }
                    }

                    if (user == null)
                    {
                        // If the user is already logged in, use the current user
                        user = await this.userManager.GetUserAsync(this.User);
                    }

                    // Add the login if we found a user
                    if (user != null)
                    {
                        var login = new UserLoginInfo(assertionGrantHandler.Name, validationResult.ExternalUserId, assertionGrantHandler.Name);
                        var addLoginResult = await this.userManager.AddLoginAsync(user, login);
                        if (!addLoginResult.Succeeded)
                        {
                            var properties = new AuthenticationProperties(new Dictionary<string, string>
                            {
                                [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = string.Join(" ", addLoginResult.Errors.Select(error => error.Description)),
                            });

                            return this.Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                        }
                    }
                    else
                    {
                        // Ask the user to create an account.
                        var properties = new AuthenticationProperties(new Dictionary<string, string>
                        {
                            [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.AccountSelectionRequired,
                        });

                        return this.Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                    }
                }

                // Ensure the user is allowed to sign in.
                if (!await this.signInManager.CanSignInAsync(user))
                {
                    var properties = new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is no longer allowed to sign in.",
                    });

                    return this.Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                }

                return await this.SignInAsync(request, user);
            }

            throw new NotImplementedException("The specified grant type is not implemented.");
        }

        private async Task<ActionResult> SignInAsync(OpenIddictRequest request, ApplicationUser user)
        {
            // Create a new ClaimsPrincipal containing the claims that
            // will be used to create an id_token, a token or a code.
            var principal = await this.signInManager.CreateUserPrincipalAsync(user);

            // Set the list of scopes granted to the client application.
            // Note: the offline_access scope must be granted
            // to allow OpenIddict to return a refresh token.
            principal.SetScopes(AllowedRefreshTokenScopes.Intersect(request.GetScopes()));

            // Note: by default, claims are NOT automatically included in the access and identity tokens.
            // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
            // whether they should be included in access tokens, in identity tokens or in both.
            foreach (var claim in principal.Claims)
            {
                // Never include the security stamp in the access and identity tokens, as it's a secret value.
                if (claim.Type == this.identityOptions.Value.ClaimsIdentity.SecurityStampClaimType)
                {
                    continue;
                }

                var destinations = new List<string>
                {
                    OpenIddictConstants.Destinations.AccessToken,
                };

                // Only add the iterated claim to the id_token if the corresponding scope was granted to the client application.
                // The other claims will only be added to the access_token, which is encrypted when using the default format.
                if ((claim.Type == OpenIddictConstants.Claims.Name && principal.HasScope(OpenIddictConstants.Scopes.Profile))
                    || (claim.Type == OpenIddictConstants.Claims.Email && principal.HasScope(OpenIddictConstants.Scopes.Email))
                    || (claim.Type == OpenIddictConstants.Claims.Role && principal.HasScope(OpenIddictConstants.Scopes.Roles)))
                {
                    destinations.Add(OpenIddictConstants.Destinations.IdentityToken);
                }

                claim.SetDestinations(destinations);
            }

            return this.SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }
    }
}
