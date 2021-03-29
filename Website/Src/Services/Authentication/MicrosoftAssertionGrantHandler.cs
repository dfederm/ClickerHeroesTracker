// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Website.Models.Authentication;

namespace Website.Services.Authentication
{
    public sealed class MicrosoftAssertionGrantHandler : IAssertionGrantHandler
    {
        private const string DiscoveryEndpoint = "https://login.microsoftonline.com/common/v2.0/.well-known/openid-configuration";

        private readonly AuthenticationSettings _authenticationSettings;

        private readonly ConfigurationManager<OpenIdConnectConfiguration> _configManager;

        public MicrosoftAssertionGrantHandler(
            IOptions<AuthenticationSettings> authenticationSettings,
            HttpClient httpClient)
        {
            _authenticationSettings = authenticationSettings.Value;

            _configManager = new ConfigurationManager<OpenIdConnectConfiguration>(DiscoveryEndpoint, new OpenIdConnectConfigurationRetriever(), httpClient);
        }

        public static string GrantType => "urn:ietf:params:oauth:grant-type:microsoft_identity_token";

        public string Name => "Microsoft";

        public async Task<AssertionGrantResult> ValidateAsync(string assertion)
        {
            OpenIdConnectConfiguration config = await _configManager.GetConfigurationAsync();

            JwtSecurityTokenHandler handler = new();
            TokenValidationParameters validationParameters = new()
            {
                ValidAudience = _authenticationSettings.Microsoft.ClientId,
                IssuerSigningKeys = config.JsonWebKeySet.Keys,

                // We cannot validate issuer normally in multi-tenant scenarios.
                ValidateIssuer = false,
            };

            try
            {
                if (handler.CanReadToken(assertion))
                {
                    handler.ValidateToken(assertion, validationParameters, out SecurityToken validatedToken);
                    if (validatedToken is JwtSecurityToken jwtToken && !string.IsNullOrEmpty(jwtToken.Subject))
                    {
                        string email = jwtToken.Claims.FirstOrDefault(claim => claim.Type.Equals("email", StringComparison.OrdinalIgnoreCase))?.Value;
                        return new AssertionGrantResult { ExternalUserId = jwtToken.Subject, ExternalUserEmail = email };
                    }
                }
            }

            // It's not great to catch all exceptions, but the Jwt middleware does the same thing
            catch (Exception)
            {
                // Just swallow and fall through to returning the error below
            }

            return new AssertionGrantResult { Error = "Token validation failed" };
        }
    }
}
