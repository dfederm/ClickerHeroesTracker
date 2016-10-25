// <copyright file="HttpRequestMessageExtensions.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.IntegrationTests.Helpers
{
    using System.Net.Http;

    internal static class HttpRequestMessageExtensions
    {
        public static void AuthenticateUser(this HttpRequestMessage request)
        {
            request.Headers.Add("Authorization", "Mock 00000000-0000-0000-0000-000000000000:Test User:");
        }

        public static void AuthenticateAdmin(this HttpRequestMessage request)
        {
            request.Headers.Add("Authorization", "Mock 00000000-0000-0000-0000-000000000000:Test User:Admin");
        }
    }
}
