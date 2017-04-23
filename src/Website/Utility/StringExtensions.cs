// <copyright file="StringExtensions.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Utility
{
    using System;

    public static class StringExtensions
    {
        public static string ToCamelCase(this string str)
        {
            if (str == null || str.Length == 0 || char.IsLower(str[0]))
            {
                return str;
            }

            return char.ToLowerInvariant(str[0]) + str.Substring(1);
        }

        public static TEnum SafeParseEnum<TEnum>(this string str)
            where TEnum : struct
        {
            TEnum value;
            return Enum.TryParse<TEnum>(str, out value) ? value : default(TEnum);
        }

        public static string[] SplitIntoChunks(this string str, int chunkLength)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            if (chunkLength < 1)
            {
                throw new ArgumentException($"{nameof(chunkLength)} must be > 0", nameof(chunkLength));
            }

            var chunks = new string[(str.Length / chunkLength) + (str.Length % chunkLength == 0 ? 0 : 1)];
            for (var i = 0; i < chunks.Length; i++)
            {
                var currentChunkStart = i * chunkLength;
                var currentChunkLength = chunkLength + currentChunkStart > str.Length
                    ? str.Length - currentChunkStart
                    : chunkLength;

                chunks[i] = str.Substring(currentChunkStart, currentChunkLength);
            }

            return chunks;
        }
    }
}