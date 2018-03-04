// <copyright file="AssertionGrantResult.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace Website.Services.Authentication
{
    public sealed class AssertionGrantResult
    {
        public string ExternalUserId { get; set; }

        public string ExternalUserEmail { get; set; }

        public string Error { get; set; }

        public bool IsSuccessful => string.IsNullOrEmpty(this.Error)
            && !string.IsNullOrEmpty(this.ExternalUserId)
            && !string.IsNullOrEmpty(this.ExternalUserEmail);
    }
}
