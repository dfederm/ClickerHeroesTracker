// <copyright file="AssertionGrantResultTests.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace UnitTests.Services.Authentication
{
    using Website.Services.Authentication;
    using Xunit;

    public sealed class AssertionGrantResultTests
    {
        [Fact]
        public void IsSuccessful_ExternalUserIdOnly()
        {
            var result = new AssertionGrantResult
            {
                ExternalUserId = "SomeExternalUserId",
            };

            Assert.True(result.IsSuccessful);
        }

        [Fact]
        public void IsSuccessful_ErrorOnly()
        {
            var result = new AssertionGrantResult
            {
                Error = "SomeError",
            };

            Assert.False(result.IsSuccessful);
        }

        [Fact]
        public void IsSuccessful_BothSet()
        {
            var result = new AssertionGrantResult
            {
                ExternalUserId = "SomeExternalUserId",
                Error = "SomeError",
            };

            Assert.False(result.IsSuccessful);
        }
    }
}
