// <copyright file="ExternalLogin.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace Website.Models.Api.Users
{
    public sealed class ExternalLogin
    {
        public string ProviderName { get; set; }

        public string ExternalUserId { get; set; }
    }
}
