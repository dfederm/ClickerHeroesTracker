// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using Website.Services.Authentication;
using Xunit;

namespace UnitTests.Services.Authentication
{
    public sealed class AssertionGrantResultTests
    {
        [Theory]
        [InlineData(null, "email")]
        [InlineData("", "email")]
        [InlineData("id", null)]
        [InlineData("id", "")]
        public void FromIdentity_MissingIdentityIsRejected(string externalUserId, string externalUserEmail)
        {
            AssertionGrantResult result = AssertionGrantResult.FromIdentity(externalUserId, externalUserEmail);
            AssertionGrantError error = Assert.IsType<AssertionGrantError>(result.Value);
            Assert.Null(error.Error);
        }
    }
}
