// <copyright file="SavedGame.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.SaveData
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using Ionic.Zlib;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Model for top-level save game data.
    /// </summary>
    /// <remarks>
    /// Ensure the logic here stays in sync with savedGame.ts on the client.
    /// </remarks>
    public class SavedGame
    {
        private const string SprinkleAntiCheatCode = "Fe12NAfA3R6z4k0z";

        private const string SprinkleSalt = "af0ik392jrmt0nsfdghy0";

        private const string AndroidPrefix = "ClickerHeroesAccountSO";

        private const int HashLength = 32;

        private const string ZlibHash = "7a990d405d2c6fb93aa8fbb0ec1a3b23";

        private static readonly Dictionary<string, EncodingAlgorithm> EncodingAlgorithmHashes = new Dictionary<string, EncodingAlgorithm>(StringComparer.OrdinalIgnoreCase)
        {
            { ZlibHash, EncodingAlgorithm.Zlib },
        };

        private static readonly Dictionary<EncodingAlgorithm, Func<string, TextReader>> DecodeFuncs = new Dictionary<EncodingAlgorithm, Func<string, TextReader>>
        {
            { EncodingAlgorithm.Sprinkle, DecodeSprinkle },
            { EncodingAlgorithm.Android, DecodeAndroid },
            { EncodingAlgorithm.Zlib, DecodeZlib },
        };

        private static readonly Dictionary<EncodingAlgorithm, Func<string, string>> EncodeFuncs = new Dictionary<EncodingAlgorithm, Func<string, string>>
        {
            { EncodingAlgorithm.Sprinkle, EncodeSprinkle },
            { EncodingAlgorithm.Android, EncodeAndroid },
            { EncodingAlgorithm.Zlib, EncodeZlib },
        };

        private readonly EncodingAlgorithm encoding;

        private SavedGame(string content)
        {
            this.Content = content;
            this.encoding = DetermineEncodingAlgorithm(content);
        }

        private enum EncodingAlgorithm
        {
            Unknown,
            Sprinkle,
            Android,
            Zlib,
        }

        public string Content { get; private set; }

        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", Justification = "It makes sense here.")]
        public JObject Object { get; private set; }

        public static SavedGame Parse(string encodedSaveData)
        {
            var savedGame = new SavedGame(encodedSaveData);

            if (!DecodeFuncs.TryGetValue(savedGame.encoding, out var decodeFunc))
            {
                return null;
            }

            // Decode the save
            using (var reader = decodeFunc(savedGame.Content))
            {
                if (reader == null)
                {
                    return null;
                }

                // Deserialize the save
                try
                {
                    using (var jsonTextReader = new JsonTextReader(reader))
                    {
                        savedGame.Object = JObject.Load(jsonTextReader);
                    }
                }
                catch (Exception)
                {
                    // Just return null for any invalid save
                    return null;
                }
            }

            return savedGame;
        }

        public static string ScrubIdentity(string encodedSaveData)
        {
            var savedGame = Parse(encodedSaveData);
            if (savedGame == null)
            {
                return null;
            }

            // Based on https://github.com/Legocro/Clan-stripper/blob/master/script.js
            savedGame.Object.Remove("type");
            savedGame.Object.Property("email", StringComparison.OrdinalIgnoreCase).Value = string.Empty;
            savedGame.Object.Property("passwordHash", StringComparison.OrdinalIgnoreCase).Value = string.Empty;
            savedGame.Object.Property("prevLoginTimestamp", StringComparison.OrdinalIgnoreCase).Value = 0;
            savedGame.Object.Remove("account");
            savedGame.Object.Property("accountId", StringComparison.OrdinalIgnoreCase).Value = 0;
            savedGame.Object.Property("loginValidated", StringComparison.OrdinalIgnoreCase).Value = false;
            savedGame.Object.Property("uniqueId", StringComparison.OrdinalIgnoreCase).Value = string.Empty;

            if (!EncodeFuncs.TryGetValue(savedGame.encoding, out var encodeFunc))
            {
                throw new InvalidOperationException($"Could not find an encoding function for {savedGame.encoding}");
            }

            var newJson = savedGame.Object.ToString(Formatting.None);
            return encodeFunc(newJson);
        }

        private static EncodingAlgorithm DetermineEncodingAlgorithm(string content)
        {
            if (content == null || content.Length < HashLength)
            {
                return EncodingAlgorithm.Unknown;
            }

            // Read the first 32 characters as they are the MD5 hash of the used algorithm
            var encodingAlgorithmHash = content.Substring(0, HashLength);

            // Test if the MD5 hash header corresponds to a known encoding algorithm
            if (EncodingAlgorithmHashes.TryGetValue(encodingAlgorithmHash, out var encodingAlgorithm))
            {
                return encodingAlgorithm;
            }

            // Legacy encodings
            return content.IndexOf(AndroidPrefix, StringComparison.OrdinalIgnoreCase) >= 0
                ? EncodingAlgorithm.Android
                : EncodingAlgorithm.Sprinkle;
        }

        [SuppressMessage("Microsoft.Security.Cryptography", "CA5351:Do not use insecure cryptographic algorithm MD5", Justification = "The encoding algorithm requires MD5. It's not used for security.")]
        private static TextReader DecodeSprinkle(string encodedSaveData)
        {
            var antiCheatCodeIndex = encodedSaveData.IndexOf(SprinkleAntiCheatCode, StringComparison.Ordinal);
            if (antiCheatCodeIndex == -1)
            {
                // Couldn't find anti-cheat
                return null;
            }

            // Remove every other character, AKA "unsprinkle".
            // Handle odd lengthed strings even though it shouldn't happen.
            var unsprinkledChars = new char[(antiCheatCodeIndex / 2) + (antiCheatCodeIndex % 2)];
            for (var i = 0; i < antiCheatCodeIndex; i += 2)
            {
                unsprinkledChars[i / 2] = encodedSaveData[i];
            }

            // Validation
            var expectedHashStart = antiCheatCodeIndex + SprinkleAntiCheatCode.Length;
            var saltedChars = new char[unsprinkledChars.Length + SprinkleSalt.Length];
            unsprinkledChars.CopyTo(saltedChars, 0);
            SprinkleSalt.CopyTo(0, saltedChars, unsprinkledChars.Length, SprinkleSalt.Length);
            using (var md5 = MD5.Create())
            {
                var data = md5.ComputeHash(Encoding.UTF8.GetBytes(saltedChars));
                for (var i = 0; i < data.Length; i++)
                {
                    var expectedHashPartIndex = expectedHashStart + (i * 2);
                    var actualHashPart = data[i].ToString("x2");
                    if (actualHashPart[0] != encodedSaveData[expectedHashPartIndex]
                        || actualHashPart[1] != encodedSaveData[expectedHashPartIndex + 1])
                    {
                        // Hash didn't match
                        return null;
                    }
                }
            }

            // Decode
            var bytes = Convert.FromBase64CharArray(unsprinkledChars, 0, unsprinkledChars.Length);

            // Wrap in a text reader
            return new StreamReader(new MemoryStream(bytes));
        }

        [SuppressMessage("Microsoft.Security.Cryptography", "CA5351:Do not use insecure cryptographic algorithm MD5", Justification = "The encoding algorithm requires MD5. It's not used for security.")]
        private static string EncodeSprinkle(string json)
        {
            var bytes = Encoding.UTF8.GetBytes(json);
            var data = Convert.ToBase64String(bytes);

            var sb = new StringBuilder((2 * data.Length) + SprinkleAntiCheatCode.Length);

            // Inject an arbitrary character every other character, AKA "sprinkle".
            for (var i = 0; i < data.Length; i++)
            {
                sb.Append(data[i]);
                sb.Append("0");
            }

            sb.Append(SprinkleAntiCheatCode);

            var hashChars = new char[data.Length + SprinkleSalt.Length];
            data.CopyTo(0, hashChars, 0, data.Length);
            SprinkleSalt.CopyTo(0, hashChars, data.Length, SprinkleSalt.Length);
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(hashChars));
                for (var i = 0; i < hash.Length; i++)
                {
                    sb.Append(hash[i].ToString("x2"));
                }
            }

            return sb.ToString();
        }

        private static TextReader DecodeAndroid(string encodedSaveData)
        {
            // Get the index of the first open brace
            const char BraceCharCode = '{';
            var firstBrace = encodedSaveData.IndexOf(BraceCharCode, StringComparison.Ordinal);
            if (firstBrace < 0)
            {
                return null;
            }

            var json = encodedSaveData
                .Substring(firstBrace)
                .Replace("\\\"", "\"", StringComparison.Ordinal)
                .Replace("\\\\", "\\", StringComparison.Ordinal);

            // Wrap in a text reader
            return new StringReader(json);
        }

        private static string EncodeAndroid(string json)
        {
            var escapedJson = json
                .Replace("\\", "\\\\", StringComparison.Ordinal)
                .Replace("\"", "\\\"", StringComparison.Ordinal);

            // No idea how accurate these chracters are. Just using what was found in one particular save.
            return "?D?TCSO" + AndroidPrefix + "\tjson??%" + escapedJson;
        }

        private static TextReader DecodeZlib(string encodedSaveData)
        {
            var data = encodedSaveData.Substring(HashLength);
            var bytes = Convert.FromBase64String(data);
            var memStream = new MemoryStream(bytes);
            var zlibStream = new ZlibStream(memStream, CompressionMode.Decompress);
            return new StreamReader(zlibStream);
        }

        private static string EncodeZlib(string json)
        {
            using (var jsonStream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            using (var zlibStream = new ZlibStream(jsonStream, CompressionMode.Compress))
            using (var compressedStream = new MemoryStream(16 * 1024))
            {
                zlibStream.CopyTo(compressedStream);
                return ZlibHash + Convert.ToBase64String(compressedStream.ToArray());
            }
        }
    }
}