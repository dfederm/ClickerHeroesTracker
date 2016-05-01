// <copyright file="Extensions.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models
{
    using System;

    public static class StringExtensions
    {
        public static string ToCamelCase(this string str)
        {
            if (str == null || str.Length == 0 || Char.IsLower(str[0]))
            {
                return str;
            }

            return Char.ToLowerInvariant(str[0]) + str.Substring(1);
        }
    }
}