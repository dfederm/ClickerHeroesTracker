// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

namespace Website.Services.Authentication
{
    public sealed class AssertionGrantResult
    {
        public string ExternalUserId { get; set; }

        public string ExternalUserEmail { get; set; }

        public string Error { get; set; }

        public bool IsSuccessful => string.IsNullOrEmpty(Error)
            && !string.IsNullOrEmpty(ExternalUserId)
            && !string.IsNullOrEmpty(ExternalUserEmail);
    }
}
