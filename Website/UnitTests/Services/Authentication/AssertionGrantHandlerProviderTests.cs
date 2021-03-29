// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using Website.Services.Authentication;
using Xunit;

namespace UnitTests.Services.Authentication
{
    public sealed class AssertionGrantHandlerProviderTests
    {
        private const string GrantType = "SomeGrantType";

        private static readonly IHttpContextAccessor HttpContextAccessor = CreateHttpContextAccessor();

        private readonly IOptions<AssertionGrantOptions> _options = Options.Create(new AssertionGrantOptions());

        [Fact]
        public void GetHandler_MissingHandler()
        {
            AssertionGrantHandlerProvider provider = new(_options, HttpContextAccessor);

            Assert.Null(provider.GetHandler(GrantType));
        }

        [Fact]
        public void GetHandler_FoundHandler()
        {
            _options.Value.AddAssertionGrantType<MockAssertionGrantHandler>(GrantType);

            AssertionGrantHandlerProvider provider = new(_options, HttpContextAccessor);

            IAssertionGrantHandler handler = provider.GetHandler(GrantType);
            Assert.NotNull(handler);
            Assert.IsType<MockAssertionGrantHandler>(handler);
        }

        private static IHttpContextAccessor CreateHttpContextAccessor()
        {
            Mock<HttpContext> mockHttpContext = new(MockBehavior.Strict);
            mockHttpContext
                .Setup(_ => _.RequestServices.GetService(typeof(MockAssertionGrantHandler)))
                .Returns(() => new MockAssertionGrantHandler());

            // HttpContextAccessor.set_HttpContent calls HttpContext.TraceIdentifier
            mockHttpContext
                .Setup(_ => _.TraceIdentifier)
                .Returns("SomeTraceIdentifier");

            return new HttpContextAccessor { HttpContext = mockHttpContext.Object };
        }

        private sealed class MockAssertionGrantHandler : IAssertionGrantHandler
        {
            public string Name => throw new NotImplementedException();

            public Task<AssertionGrantResult> ValidateAsync(string assertion) => throw new NotImplementedException();
        }
    }
}
