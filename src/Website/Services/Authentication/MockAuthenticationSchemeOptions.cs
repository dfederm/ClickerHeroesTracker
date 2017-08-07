// <copyright file="MockAuthenticationSchemeOptions.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Services.Authentication
{
    using Microsoft.AspNetCore.Authentication;

    /// <summary>
    /// Authentication handler which mocks the authentication with the identity data from the request.
    /// </summary>
    internal sealed class MockAuthenticationSchemeOptions : AuthenticationSchemeOptions
    {
        public bool IsEnabled { get; set; }
    }
}