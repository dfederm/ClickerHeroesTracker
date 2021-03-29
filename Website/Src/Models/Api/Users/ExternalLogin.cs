// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

namespace Website.Models.Api.Users
{
    public sealed class ExternalLogin
    {
        public string ProviderName { get; set; }

        public string ExternalUserId { get; set; }
    }
}
