// <copyright file="AssertionGrantHandlerProvider.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace Website.Services.Authentication
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;

    public sealed class AssertionGrantHandlerProvider : IAssertionGrantHandlerProvider
    {
        private readonly AssertionGrantOptions options;
        private readonly IHttpContextAccessor httpContextAccessor;

        public AssertionGrantHandlerProvider(IOptions<AssertionGrantOptions> options, IHttpContextAccessor httpContextAccessor)
        {
            this.options = options.Value;
            this.httpContextAccessor = httpContextAccessor;
        }

        public IAssertionGrantHandler GetHandler(string grantType)
        {
            return this.options.AssertionGrantTypeMap.TryGetValue(grantType, out var handlerType)
                ? this.httpContextAccessor.HttpContext.RequestServices.GetService(handlerType) as IAssertionGrantHandler
                : null;
        }
    }
}
