// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

namespace Website.Services.Authentication
{
    public sealed record AssertionGrantSuccess(string ExternalUserId, string ExternalUserEmail);
}