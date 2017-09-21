// <copyright file="AssertionGrantHandlerProviderTests.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace UnitTests.Services.Authentication
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;
    using Moq;
    using Website.Services.Authentication;
    using Xunit;

    public sealed class AssertionGrantHandlerProviderTests
    {
        private const string GrantType = "SomeGrantType";

        private static IHttpContextAccessor httpContextAccessor = CreateHttpContextAccessor();

        private IOptions<AssertionGrantOptions> options = Options.Create(new AssertionGrantOptions());

        [Fact]
        public void GetHandler_MissingHandler()
        {
            var provider = new AssertionGrantHandlerProvider(this.options, httpContextAccessor);

            Assert.Null(provider.GetHandler(GrantType));
        }

        [Fact]
        public void GetHandler_FoundHandler()
        {
            this.options.Value.AddAssertionGrantType<MockAssertionGrantHandler>(GrantType);

            var provider = new AssertionGrantHandlerProvider(this.options, httpContextAccessor);

            var handler = provider.GetHandler(GrantType);
            Assert.NotNull(handler);
            Assert.IsType<MockAssertionGrantHandler>(handler);
        }

        private static IHttpContextAccessor CreateHttpContextAccessor()
        {
            var mockHttpContext = new Mock<HttpContext>(MockBehavior.Strict);
            mockHttpContext.Setup(_ => _.RequestServices.GetService(typeof(MockAssertionGrantHandler))).Returns(() => new MockAssertionGrantHandler());
            return new HttpContextAccessor { HttpContext = mockHttpContext.Object };
        }

        private sealed class MockAssertionGrantHandler : IAssertionGrantHandler
        {
            public string Name => throw new NotImplementedException();

            public Task<AssertionGrantResult> ValidateAsync(string assertion)
            {
                throw new NotImplementedException();
            }
        }
    }
}
