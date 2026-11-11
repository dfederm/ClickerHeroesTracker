// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

namespace Website.Services.Authentication
{
    public union AssertionGrantResult(AssertionGrantSuccess, AssertionGrantError)
    {
        public static AssertionGrantResult FromIdentity(string externalUserId, string externalUserEmail)
        {
            if (string.IsNullOrEmpty(externalUserId) || string.IsNullOrEmpty(externalUserEmail))
            {
                return new AssertionGrantError(null);
            }

            return new AssertionGrantSuccess(externalUserId, externalUserEmail);
        }
    }
}
