// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ClickerHeroesTrackerWebsite.Models.SaveData
{
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

        private const string DeflateHash = "7e8bb5a89f2842ac4af01b3b7e228592";

        private static readonly Dictionary<string, EncodingAlgorithm> EncodingAlgorithmHashes = new(StringComparer.OrdinalIgnoreCase)
        {
            { ZlibHash, EncodingAlgorithm.Zlib },
            { DeflateHash, EncodingAlgorithm.Deflate },
        };

        private static readonly Dictionary<EncodingAlgorithm, Func<string, TextReader>> DecodeFuncs = new()
        {
            { EncodingAlgorithm.Sprinkle, DecodeSprinkle },
            { EncodingAlgorithm.Android, DecodeAndroid },
            { EncodingAlgorithm.Zlib, DecodeZlib },
            { EncodingAlgorithm.Deflate, DecodeDeflate },
        };

        private static readonly Dictionary<EncodingAlgorithm, Func<string, string>> EncodeFuncs = new()
        {
            { EncodingAlgorithm.Sprinkle, EncodeSprinkle },
            { EncodingAlgorithm.Android, EncodeAndroid },
            { EncodingAlgorithm.Zlib, EncodeZlib },
            { EncodingAlgorithm.Deflate, EncodeDeflate },
        };

        private readonly EncodingAlgorithm _encoding;

        private SavedGame(EncodingAlgorithm encoding, JObject obj)
        {
            _encoding = encoding;
            Object = obj;
        }

        private enum EncodingAlgorithm
        {
            Unknown,
            Sprinkle,
            Android,
            Zlib,
            Deflate,
        }

        public JObject Object { get; private set; }

        public static SavedGame Parse(string encodedSaveData)
        {
            EncodingAlgorithm encoding = DetermineEncodingAlgorithm(encodedSaveData);

            if (!DecodeFuncs.TryGetValue(encoding, out Func<string, TextReader> decodeFunc))
            {
                return null;
            }

            // Decode the save
            using (TextReader reader = decodeFunc(encodedSaveData))
            {
                if (reader == null)
                {
                    return null;
                }

                // Deserialize the save
                try
                {
                    using (JsonTextReader jsonTextReader = new(reader))
                    {
                        return new SavedGame(encoding, JObject.Load(jsonTextReader));
                    }
                }
                catch (Exception)
                {
                    // Just return null for any invalid save
                    return null;
                }
            }
        }

        public static string ScrubIdentity(string encodedSaveData)
        {
            SavedGame savedGame = Parse(encodedSaveData);
            if (savedGame == null)
            {
                return null;
            }

            // Based on https://github.com/Legocro/Clan-stripper/blob/master/script.js
            savedGame.Object.Remove("type");
            SetPropertyIfExists("email", string.Empty);
            SetPropertyIfExists("passwordHash", string.Empty);
            SetPropertyIfExists("prevLoginTimestamp", 0);
            savedGame.Object.Remove("account");
            SetPropertyIfExists("accountId", 0);
            SetPropertyIfExists("loginValidated", false);
            SetPropertyIfExists("uniqueId", string.Empty);

            void SetPropertyIfExists(string propertyName, JToken value)
            {
                JProperty property = savedGame.Object.Property(propertyName, StringComparison.OrdinalIgnoreCase);
                if (property != null)
                {
                    property.Value = value;
                }
            }

            if (!EncodeFuncs.TryGetValue(savedGame._encoding, out Func<string, string> encodeFunc))
            {
                throw new InvalidOperationException($"Could not find an encoding function for {savedGame._encoding}");
            }

            string newJson = savedGame.Object.ToString(Formatting.None);
            return encodeFunc(newJson);
        }

        private static EncodingAlgorithm DetermineEncodingAlgorithm(string content)
        {
            if (content == null || content.Length < HashLength)
            {
                return EncodingAlgorithm.Unknown;
            }

            // Read the first 32 characters as they are the MD5 hash of the used algorithm
            string encodingAlgorithmHash = content.Substring(0, HashLength);

            // Test if the MD5 hash header corresponds to a known encoding algorithm
            if (EncodingAlgorithmHashes.TryGetValue(encodingAlgorithmHash, out EncodingAlgorithm encodingAlgorithm))
            {
                return encodingAlgorithm;
            }

            // Legacy encodings
            return content.Contains(AndroidPrefix, StringComparison.OrdinalIgnoreCase)
                ? EncodingAlgorithm.Android
                : EncodingAlgorithm.Sprinkle;
        }

        private static TextReader DecodeSprinkle(string encodedSaveData)
        {
            int antiCheatCodeIndex = encodedSaveData.IndexOf(SprinkleAntiCheatCode, StringComparison.Ordinal);
            if (antiCheatCodeIndex == -1)
            {
                // Couldn't find anti-cheat
                return null;
            }

            // Remove every other character, AKA "unsprinkle".
            // Handle odd lengthed strings even though it shouldn't happen.
            char[] unsprinkledChars = new char[(antiCheatCodeIndex / 2) + (antiCheatCodeIndex % 2)];
            for (int i = 0; i < antiCheatCodeIndex; i += 2)
            {
                unsprinkledChars[i / 2] = encodedSaveData[i];
            }

            // Validation
            int expectedHashStart = antiCheatCodeIndex + SprinkleAntiCheatCode.Length;
            char[] saltedChars = new char[unsprinkledChars.Length + SprinkleSalt.Length];
            unsprinkledChars.CopyTo(saltedChars, 0);
            SprinkleSalt.CopyTo(0, saltedChars, unsprinkledChars.Length, SprinkleSalt.Length);
            using (MD5 md5 = MD5.Create())
            {
                byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(saltedChars));
                for (int i = 0; i < data.Length; i++)
                {
                    int expectedHashPartIndex = expectedHashStart + (i * 2);
                    string actualHashPart = data[i].ToString("x2");
                    if (actualHashPart[0] != encodedSaveData[expectedHashPartIndex]
                        || actualHashPart[1] != encodedSaveData[expectedHashPartIndex + 1])
                    {
                        // Hash didn't match
                        return null;
                    }
                }
            }

            // Decode
            byte[] bytes = Convert.FromBase64CharArray(unsprinkledChars, 0, unsprinkledChars.Length);

            // Wrap in a text reader
            return new StreamReader(new MemoryStream(bytes));
        }

        private static string EncodeSprinkle(string json)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            string data = Convert.ToBase64String(bytes);

            StringBuilder sb = new((2 * data.Length) + SprinkleAntiCheatCode.Length);

            // Inject an arbitrary character every other character, AKA "sprinkle".
            for (int i = 0; i < data.Length; i++)
            {
                sb.Append(data[i]);
                sb.Append('0');
            }

            sb.Append(SprinkleAntiCheatCode);

            char[] hashChars = new char[data.Length + SprinkleSalt.Length];
            data.CopyTo(0, hashChars, 0, data.Length);
            SprinkleSalt.CopyTo(0, hashChars, data.Length, SprinkleSalt.Length);
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(hashChars));
                for (int i = 0; i < hash.Length; i++)
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
            int firstBrace = encodedSaveData.IndexOf(BraceCharCode, StringComparison.Ordinal);
            if (firstBrace < 0)
            {
                return null;
            }

            string json = encodedSaveData
                .Substring(firstBrace)
                .Replace("\\\"", "\"", StringComparison.Ordinal)
                .Replace("\\\\", "\\", StringComparison.Ordinal);

            // Wrap in a text reader
            return new StringReader(json);
        }

        private static string EncodeAndroid(string json)
        {
            string escapedJson = json
                .Replace("\\", "\\\\", StringComparison.Ordinal)
                .Replace("\"", "\\\"", StringComparison.Ordinal);

            // No idea how accurate these chracters are. Just using what was found in one particular save.
            return "?D?TCSO" + AndroidPrefix + "\tjson??%" + escapedJson;
        }

        private static TextReader DecodeZlib(string encodedSaveData)
        {
            string data = encodedSaveData.Substring(HashLength);
            byte[] bytes = Convert.FromBase64String(data);
            MemoryStream memStream = new(bytes);
            InflaterInputStream inflaterStream = new(memStream);
            return new StreamReader(inflaterStream);
        }

        private static string EncodeZlib(string json)
        {
            using (MemoryStream compressedStream = new(16 * 1024))
            {
                using (MemoryStream jsonStream = new(Encoding.UTF8.GetBytes(json)))
                using (DeflaterOutputStream deflaterStream = new(compressedStream))
                {
                    jsonStream.CopyTo(deflaterStream);
                }

                return ZlibHash + Convert.ToBase64String(compressedStream.ToArray());
            }
        }

        private static TextReader DecodeDeflate(string encodedSaveData)
        {
            string data = encodedSaveData.Substring(HashLength);
            byte[] bytes = Convert.FromBase64String(data);
            MemoryStream memStream = new(bytes);
            InflaterInputStream inflaterStream = new(memStream, new Inflater(noHeader: true));
            return new StreamReader(inflaterStream);
        }

        private static string EncodeDeflate(string json)
        {
            using (MemoryStream compressedStream = new(16 * 1024))
            {
                using (MemoryStream jsonStream = new(Encoding.UTF8.GetBytes(json)))
                using (DeflaterOutputStream deflaterStream = new(compressedStream, new Deflater(level: Deflater.DEFAULT_COMPRESSION, noZlibHeaderOrFooter: true)))
                {
                    jsonStream.CopyTo(deflaterStream);
                }

                return DeflateHash + Convert.ToBase64String(compressedStream.ToArray());
            }
        }
    }
}