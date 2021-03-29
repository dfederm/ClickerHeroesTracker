// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Website.Services.Authentication
{
    public sealed class AssertionGrantHandlerProvider : IAssertionGrantHandlerProvider
    {
        private readonly AssertionGrantOptions _options;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AssertionGrantHandlerProvider(IOptions<AssertionGrantOptions> options, IHttpContextAccessor httpContextAccessor)
        {
            _options = options.Value;
            _httpContextAccessor = httpContextAccessor;
        }

        public IAssertionGrantHandler GetHandler(string grantType)
            => _options.AssertionGrantTypeMap.TryGetValue(grantType, out System.Type handlerType)
                ? _httpContextAccessor.HttpContext.RequestServices.GetService(handlerType) as IAssertionGrantHandler
                : null;
    }
}
