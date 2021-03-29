// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System.Collections.Generic;

namespace Website.Models.Api.Users
{
    public sealed class UserLogins
    {
        public bool HasPassword { get; set; }

        public IList<ExternalLogin> ExternalLogins { get; set; }
    }
}
