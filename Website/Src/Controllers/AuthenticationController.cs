// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

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
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace ClickerHeroesTrackerWebsite.Controllers
{
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

        private readonly IOptions<IdentityOptions> _identityOptions;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAssertionGrantHandlerProvider _assertionGrantHandlerProvider;

        public AuthenticationController(
            IOptions<IdentityOptions> identityOptions,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IAssertionGrantHandlerProvider assertionGrantHandlerProvider)
        {
            _identityOptions = identityOptions;
            _signInManager = signInManager;
            _userManager = userManager;
            _assertionGrantHandlerProvider = assertionGrantHandlerProvider;
        }

        [HttpPost("token")]
        [Produces("application/json")]
        [Authorize] // Authorize + AllowAnonymous to basically force authentication to work without requiring it. There's probably a better way for this to work...
        [AllowAnonymous]
        public async Task<ActionResult> ExchangeAsync()
        {
            OpenIddictRequest request = HttpContext.GetOpenIddictServerRequest();

            if (request.IsPasswordGrantType())
            {
                // Allow the user to log in with their email address too.
                // We already check that usernames are only "word" chars (\w+), so this check is sufficient.
                ApplicationUser user = request.Username.Contains("@", StringComparison.Ordinal)
                    ? await _userManager.FindByEmailAsync(request.Username)
                    : await _userManager.FindByNameAsync(request.Username);
                if (user == null)
                {
                    AuthenticationProperties properties = new(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The username/password couple is invalid.",
                    });

                    return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                }

                // Validate the username/password parameters and ensure the account is not locked out.
                SignInResult result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
                if (!result.Succeeded)
                {
                    AuthenticationProperties properties = new(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The username/password couple is invalid.",
                    });

                    return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                }

                return await SignInAsync(request, user);
            }

            if (request.IsRefreshTokenGrantType())
            {
                // Retrieve the claims principal stored in the refresh token.
                AuthenticateResult result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

                // Retrieve the user profile corresponding to the refresh token.
                ApplicationUser user = await _signInManager.ValidateSecurityStampAsync(result.Principal);
                if (user == null)
                {
                    AuthenticationProperties properties = new(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The refresh token is no longer valid.",
                    });

                    return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                }

                // Ensure the user is still allowed to sign in.
                if (!await _signInManager.CanSignInAsync(user))
                {
                    AuthenticationProperties properties = new(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is no longer allowed to sign in.",
                    });

                    return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                }

                // Reuse the properties stored in the refresh token, including the scopes originally granted.
                return await SignInAsync(request, user);
            }

            IAssertionGrantHandler assertionGrantHandler = _assertionGrantHandlerProvider.GetHandler(request.GrantType);
            if (assertionGrantHandler != null)
            {
                // Reject the request if the "assertion" parameter is missing.
                if (string.IsNullOrEmpty(request.Assertion))
                {
                    AuthenticationProperties properties = new(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidRequest,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The mandatory 'assertion' parameter was missing.",
                    });

                    return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                }

                AssertionGrantResult validationResult = await assertionGrantHandler.ValidateAsync(request.Assertion);
                if (!validationResult.IsSuccessful)
                {
                    AuthenticationProperties properties = new(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = validationResult.Error,
                    });

                    return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                }

                // Find the user associated with this external log in
                ApplicationUser user = await _userManager.FindByLoginAsync(assertionGrantHandler.Name, validationResult.ExternalUserId);
                if (user == null)
                {
                    if (!string.IsNullOrEmpty(request.Username))
                    {
                        // They provided a user name, so try to implicitly create an account for them
                        user = new ApplicationUser { UserName = request.Username, Email = validationResult.ExternalUserEmail };
                        IdentityResult creationResult = await _userManager.CreateAsync(user);
                        if (!creationResult.Succeeded)
                        {
                            AuthenticationProperties properties = new(new Dictionary<string, string>
                            {
                                [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = string.Join(" ", creationResult.Errors.Select(error => error.Description)),
                            });

                            return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                        }
                    }

                    if (user == null)
                    {
                        // If the user is already logged in, use the current user
                        user = await _userManager.GetUserAsync(User);
                    }

                    // Add the login if we found a user
                    if (user != null)
                    {
                        UserLoginInfo login = new(assertionGrantHandler.Name, validationResult.ExternalUserId, assertionGrantHandler.Name);
                        IdentityResult addLoginResult = await _userManager.AddLoginAsync(user, login);
                        if (!addLoginResult.Succeeded)
                        {
                            AuthenticationProperties properties = new(new Dictionary<string, string>
                            {
                                [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = string.Join(" ", addLoginResult.Errors.Select(error => error.Description)),
                            });

                            return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                        }
                    }
                    else
                    {
                        // Ask the user to create an account.
                        AuthenticationProperties properties = new(new Dictionary<string, string>
                        {
                            [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.AccountSelectionRequired,
                        });

                        return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                    }
                }

                // Ensure the user is allowed to sign in.
                if (!await _signInManager.CanSignInAsync(user))
                {
                    AuthenticationProperties properties = new(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is no longer allowed to sign in.",
                    });

                    return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                }

                return await SignInAsync(request, user);
            }

            throw new NotImplementedException("The specified grant type is not implemented.");
        }

        private async Task<ActionResult> SignInAsync(OpenIddictRequest request, ApplicationUser user)
        {
            // Create a new ClaimsPrincipal containing the claims that
            // will be used to create an id_token, a token or a code.
            ClaimsPrincipal principal = await _signInManager.CreateUserPrincipalAsync(user);

            // Set the list of scopes granted to the client application.
            // Note: the offline_access scope must be granted
            // to allow OpenIddict to return a refresh token.
            principal.SetScopes(AllowedRefreshTokenScopes.Intersect(request.GetScopes()));

            // Note: by default, claims are NOT automatically included in the access and identity tokens.
            // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
            // whether they should be included in access tokens, in identity tokens or in both.
            foreach (Claim claim in principal.Claims)
            {
                // Never include the security stamp in the access and identity tokens, as it's a secret value.
                if (claim.Type == _identityOptions.Value.ClaimsIdentity.SecurityStampClaimType)
                {
                    continue;
                }

                List<string> destinations = new()
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

            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }
    }
}
