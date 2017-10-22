// <copyright file="UserLogins.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace Website.Models.Api.Users
{
    using System.Collections.Generic;

    public sealed class UserLogins
    {
        public bool HasPassword { get; set; }

        public IList<string> ExternalLogins { get; set; }
    }
}
