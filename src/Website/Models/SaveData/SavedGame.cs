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
    using Newtonsoft.Json;

    /// <summary>
    /// Model for top-level save game data.
    /// </summary>
    [JsonObject]
    public class SavedGame
    {
        private static readonly JsonSerializer Serializer = CreateSerializer();

        /// <summary>
        /// Gets or sets the ancients data for the saved game.
        /// </summary>
        [JsonProperty(PropertyName = "ancients")]
        public AncientsData AncientsData { get; set; }

        /// <summary>
        /// Gets or sets the items data for the saved game.
        /// </summary>
        [JsonProperty(PropertyName = "items")]
        public ItemsData ItemsData { get; set; }

        /// <summary>
        /// Gets or sets the items data for the saved game.
        /// </summary>
        [JsonProperty(PropertyName = "outsiders")]
        public OutsidersData OutsidersData { get; set; }

        /// <summary>
        /// Gets or sets the user's titan damage.
        /// </summary>
        [JsonProperty(PropertyName = "titanDamage")]
        public double TitanDamage { get; set; }

        /// <summary>
        /// Gets or sets the total ancient souls earned
        /// </summary>
        [JsonProperty(PropertyName = "ancientSoulsTotal")]
        public double AncientSoulsTotal { get; set; }

        /// <summary>
        /// Gets or sets the highest finished zone ever
        /// </summary>
        [JsonProperty(PropertyName = "transcendentHighestFinishedZone")]
        public double TranscendentHighestFinishedZone { get; set; }

        /// <summary>
        /// Gets or sets the highest finished zone ever
        /// </summary>
        [JsonProperty(PropertyName = "pretranscendentHighestFinishedZone")]
        public double PretranscendentHighestFinishedZone { get; set; }

        /// <summary>
        /// Gets or sets the highest finished zone in this transcendence
        /// </summary>
        [JsonProperty(PropertyName = "highestFinishedZonePersist")]
        public double HighestFinishedZonePersist { get; set; }

        /// <summary>
        /// Gets or sets the hero souls sacrificed in transcendence
        /// </summary>
        [JsonProperty(PropertyName = "heroSoulsSacrificed")]
        public double HeroSoulsSacrificed { get; set; }

        /// <summary>
        /// Gets or sets the number of rubies
        /// </summary>
        [JsonProperty(PropertyName = "rubies")]
        public double Rubies { get; set; }

        /// <summary>
        /// Gets or sets the number of ascensions this transcension
        /// </summary>
        [JsonProperty(PropertyName = "numAscensionsThisTranscension")]
        public double NumAscensionsThisTranscension { get; set; }

        /// <summary>
        /// Gets or sets the number of ascensions ever
        /// </summary>
        [JsonProperty(PropertyName = "numWorldResets")]
        public double NumWorldResets { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user has transcended yet.
        /// </summary>
        [JsonProperty(PropertyName = "transcendent")]
        public bool Transcendent { get; set; }

        /// <summary>
        /// Gets or sets unique Id for the user.
        /// </summary>
        [JsonProperty(PropertyName = "uniqueId")]
        public string UniqueId { get; set; }

        /// <summary>
        /// Gets or sets Password Hash for the user.
        /// </summary>
        [JsonProperty(PropertyName = "passwordHash")]
        public string PasswordHash { get; set; }

        /// <summary>
        /// Gets or sets current number of souls.
        /// </summary>
        [JsonProperty(PropertyName = "heroSouls")]
        public double HeroSouls { get; set; }

        /// <summary>
        /// Gets or sets number of souls earned upon ascending.
        /// </summary>
        [JsonProperty(PropertyName = "primalSouls")]
        public double PendingSouls { get; set; }

        /// <summary>
        /// Parsed the encoded save game data to a structured object.
        /// </summary>
        /// <param name="encodedSaveData">The encoded saved game</param>
        /// <returns>The structured saved game data</returns>
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
            return IsAndroid(encodedSaveData)
                ? DecodeAndroidSaveData(encodedSaveData)
                : DecodeWebSaveData(encodedSaveData);
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
            var antiCheatCodeIndex = encodedSaveData.IndexOf(AntiCheatCode);
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