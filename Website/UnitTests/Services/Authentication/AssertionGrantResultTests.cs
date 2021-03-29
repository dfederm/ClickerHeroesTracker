// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using Website.Services.Authentication;
using Xunit;

namespace UnitTests.Services.Authentication
{
    public sealed class AssertionGrantResultTests
    {
        [Theory]
        [InlineData(false, null, null, null)]
        [InlineData(false, null, "SomeExternalUserEmail", null)]
        [InlineData(false, null, null, "SomeError")]
        [InlineData(false, null, "SomeExternalUserEmail", "SomeError")]
        [InlineData(false, "SomeExternalUserId", null, null)]
        [InlineData(true, "SomeExternalUserId", "SomeExternalUserEmail", null)]
        [InlineData(false, "SomeExternalUserId", null, "SomeError")]
        [InlineData(false, "SomeExternalUserId", "SomeExternalUserEmail", "SomeError")]
        public void IsSuccessful(bool isSuccessful, string externalUserId, string externalUserEmail, string error)
        {
            AssertionGrantResult result = new()
            {
                ExternalUserId = externalUserId,
                ExternalUserEmail = externalUserEmail,
                Error = error,
            };

            Assert.Equal(isSuccessful, result.IsSuccessful);
        }
    }
}
