// <copyright file="SavedGame.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.SaveData
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Numerics;
    using System.Security.Cryptography;
    using System.Text;
    using ClickerHeroesTrackerWebsite.Utility;
    using Ionic.Zlib;
    using Newtonsoft.Json;

    /// <summary>
    /// Model for top-level save game data.
    /// </summary>
    [JsonObject]
    public class SavedGame
    {
        private static readonly JsonSerializer Serializer = CreateSerializer();

        private static readonly Dictionary<string, EncodingAlgorithm> EncodingAlgorithmHashes = new Dictionary<string, EncodingAlgorithm>(StringComparer.OrdinalIgnoreCase)
            {
                { "7a990d405d2c6fb93aa8fbb0ec1a3b23", EncodingAlgorithm.Zlib },
            };

        private enum EncodingAlgorithm
        {
            Sprinkle,
            Zlib,
        }

        [JsonProperty(PropertyName = "ancients")]
        public AncientsData AncientsData { get; set; }

        [JsonProperty(PropertyName = "items")]
        public ItemsData ItemsData { get; set; }

        [JsonProperty(PropertyName = "outsiders")]
        public OutsidersData OutsidersData { get; set; }

        [JsonProperty(PropertyName = "titanDamage")]
        [JsonConverter(typeof(BigIntegerStringConverter))]
        public BigInteger TitanDamage { get; set; }

        [JsonProperty(PropertyName = "ancientSoulsTotal")]
        [JsonConverter(typeof(LongFloorConverter))]
        public long AncientSoulsTotal { get; set; }

        [JsonProperty(PropertyName = "transcendentHighestFinishedZone")]
        [JsonConverter(typeof(LongFloorConverter))]
        public long TranscendentHighestFinishedZone { get; set; }

        [JsonProperty(PropertyName = "pretranscendentHighestFinishedZone")]
        [JsonConverter(typeof(LongFloorConverter))]
        public long PretranscendentHighestFinishedZone { get; set; }

        [JsonProperty(PropertyName = "highestFinishedZonePersist")]
        [JsonConverter(typeof(LongFloorConverter))]
        public long HighestFinishedZonePersist { get; set; }

        [JsonProperty(PropertyName = "heroSoulsSacrificed")]
        [JsonConverter(typeof(BigIntegerStringConverter))]
        public BigInteger HeroSoulsSacrificed { get; set; }

        [JsonProperty(PropertyName = "rubies")]
        [JsonConverter(typeof(LongFloorConverter))]
        public long Rubies { get; set; }

        [JsonProperty(PropertyName = "numAscensionsThisTranscension")]
        [JsonConverter(typeof(LongFloorConverter))]
        public long NumAscensionsThisTranscension { get; set; }

        [JsonProperty(PropertyName = "numWorldResets")]
        [JsonConverter(typeof(LongFloorConverter))]
        public long NumWorldResets { get; set; }

        [JsonProperty(PropertyName = "transcendent")]
        public bool Transcendent { get; set; }

        [JsonProperty(PropertyName = "uniqueId")]
        public string UniqueId { get; set; }

        [JsonProperty(PropertyName = "passwordHash")]
        public string PasswordHash { get; set; }

        [JsonProperty(PropertyName = "heroSouls")]
        [JsonConverter(typeof(BigIntegerStringConverter))]
        public BigInteger HeroSouls { get; set; }

        [JsonProperty(PropertyName = "primalSouls")]
        [JsonConverter(typeof(BigIntegerStringConverter))]
        public BigInteger PendingSouls { get; set; }

        [JsonProperty(PropertyName = "autoclickers")]
        [JsonConverter(typeof(LongFloorConverter))]
        public long Autoclickers { get; set; }

        public static SavedGame Parse(string encodedSaveData)
        {
            // Decode the save
            var jsonData = DecodeSaveData(encodedSaveData);
            if (jsonData == null)
            {
                return null;
            }

            // Deserialize the save
            return DeserializeSavedGame(jsonData);
        }

        internal static SavedGame DeserializeSavedGame(TextReader reader)
        {
            return Serializer.Deserialize<SavedGame>(new JsonTextReader(reader));
        }

        internal static bool IsAndroid(string encodedSaveData)
        {
            return encodedSaveData.IndexOf("ClickerHeroesAccountSO", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        internal static TextReader DecodeSaveData(string encodedSaveData)
        {
            const int HashLength = 32;
            if (encodedSaveData.Length < 32)
            {
                return null;
            }

            // Read the first 32 characters as they are the MD5 hash of the used algorithm
            var encodingAlgorithmHash = encodedSaveData.Substring(0, HashLength);

            // Test if the MD5 hash header corresponds to a known encoding algorithm
            if (!EncodingAlgorithmHashes.TryGetValue(encodingAlgorithmHash, out var encodingAlgorithm))
            {
                // Default to "sprinkle" (old-style encoding)
                encodingAlgorithm = EncodingAlgorithm.Sprinkle;
            }

            switch (encodingAlgorithm)
            {
                case EncodingAlgorithm.Zlib:
                    var compressedData = encodedSaveData.Substring(HashLength);
                    return DecodeZlib(compressedData);
                case EncodingAlgorithm.Sprinkle:
                default:
                    return IsAndroid(encodedSaveData)
                        ? DecodeAndroidSaveData(encodedSaveData)
                        : DecodeWebSaveData(encodedSaveData);
            }
        }

        private static TextReader DecodeAndroidSaveData(string encodedSaveData)
        {
            // Get the index of the first open brace
            var reader = new StringReader(encodedSaveData);

            // Skip to the first brace
            const int BraceCharCode = (int)'{';
            var currentChar = reader.Peek();
            while (currentChar != BraceCharCode)
            {
                // If we hit the end without seeing a brace, this wasn't a valid save
                if (currentChar == -1)
                {
                    return null;
                }

                reader.Read();
                currentChar = reader.Peek();
            }

            return reader;
        }

        [SuppressMessage("Microsoft.Security.Cryptography", "CA5351:Do not use insecure cryptographic algorithm MD5", Justification = "The encoding algorithm requires MD5. It's not used for security.")]
        private static TextReader DecodeWebSaveData(string encodedSaveData)
        {
            const string AntiCheatCode = "Fe12NAfA3R6z4k0z";
            var antiCheatCodeIndex = encodedSaveData.IndexOf(AntiCheatCode, StringComparison.Ordinal);
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
            const string Salt = "af0ik392jrmt0nsfdghy0";
            var expectedHashStart = antiCheatCodeIndex + AntiCheatCode.Length;
            var saltedChars = new char[unsprinkledChars.Length + Salt.Length];
            unsprinkledChars.CopyTo(saltedChars, 0);
            Salt.CopyTo(0, saltedChars, unsprinkledChars.Length, Salt.Length);
            using (MD5 md5 = MD5.Create())
            {
                byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(saltedChars));
                var actualHash = new StringBuilder(data.Length * 2);
                for (int i = 0; i < data.Length; i++)
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

        private static TextReader DecodeZlib(string compressedData)
        {
            var bytes = Convert.FromBase64String(compressedData);
            var memStream = new MemoryStream(bytes);
            var zlibStream = new ZlibStream(memStream, CompressionMode.Decompress);
            return new StreamReader(zlibStream);
        }

        private static JsonSerializer CreateSerializer()
        {
            var settings = new JsonSerializerSettings();
            settings.Error += (sender, args) =>
            {
                // Just swallow
                args.ErrorContext.Handled = true;
            };

            return JsonSerializer.Create(settings);
        }
    }
}