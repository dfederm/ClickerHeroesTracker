// <copyright file="MicrosoftAssertionGrantHandler.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace Website.Services.Authentication
{
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

    public sealed class MicrosoftAssertionGrantHandler : IAssertionGrantHandler
    {
        private const string DiscoveryEndpoint = "https://login.microsoftonline.com/common/v2.0/.well-known/openid-configuration";

        private readonly AuthenticationSettings authenticationSettings;

        private readonly ConfigurationManager<OpenIdConnectConfiguration> configManager;

        public MicrosoftAssertionGrantHandler(
            IOptions<AuthenticationSettings> authenticationSettings,
            HttpClient httpClient)
        {
            this.authenticationSettings = authenticationSettings.Value;

            this.configManager = new ConfigurationManager<OpenIdConnectConfiguration>(DiscoveryEndpoint, new OpenIdConnectConfigurationRetriever(), httpClient);
        }

        public static string GrantType => "urn:ietf:params:oauth:grant-type:microsoft_identity_token";

        public string Name => "Microsoft";

        public async Task<AssertionGrantResult> ValidateAsync(string assertion)
        {
            var config = await this.configManager.GetConfigurationAsync();

            var handler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters()
            {
                ValidAudience = this.authenticationSettings.Microsoft.ClientId,
                IssuerSigningKeys = config.JsonWebKeySet.Keys,

                // We cannot validate issuer normally in multi-tenant scenarios.
                ValidateIssuer = false,
            };

            try
            {
                if (handler.CanReadToken(assertion))
                {
                    handler.ValidateToken(assertion, validationParameters, out var validatedToken);
                    if (validatedToken is JwtSecurityToken jwtToken && !string.IsNullOrEmpty(jwtToken.Subject))
                    {
                        var email = jwtToken.Claims.FirstOrDefault(claim => claim.Type.Equals("email", StringComparison.OrdinalIgnoreCase))?.Value;
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
