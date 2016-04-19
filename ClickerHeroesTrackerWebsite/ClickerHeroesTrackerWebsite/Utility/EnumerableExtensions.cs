// <copyright file="EnumerableExtensions.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Utility
{
    using System;

    internal static class EnumerableExtensions
    {
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
