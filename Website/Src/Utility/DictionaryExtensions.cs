// <copyright file="DictionaryExtensions.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Utility
{
    using System.Collections.Generic;

    internal static class DictionaryExtensions
    {
        public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dictionay, TKey key, TValue value)
        {
            if (dictionay.ContainsKey(key))
            {
                dictionay[key] = value;
            }
            else
            {
                dictionay.Add(key, value);
            }
        }

        public static TValue SafeGet<TKey, TValue>(this IDictionary<TKey, TValue> dictionay, TKey key)
        {
            return dictionay.TryGetValue(key, out var value) ? value : default(TValue);
        }
    }
}