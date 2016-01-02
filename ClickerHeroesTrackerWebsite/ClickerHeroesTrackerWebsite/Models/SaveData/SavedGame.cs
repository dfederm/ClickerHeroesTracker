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
        [JsonProperty(PropertyName = "ancients", Required = Required.Always)]
        public AncientsData AncientsData { get; set; }

        /// <summary>
        /// Gets or sets the heroes data for the saved game.
        /// </summary>
        [JsonProperty(PropertyName = "heroCollection", Required = Required.Always)]
        public HeroesData HeroesData { get; set; }

        /// <summary>
        /// Gets or sets the items data for the saved game.
        /// </summary>
        [JsonProperty(PropertyName = "items", Required = Required.Always)]
        public ItemsData ItemsData { get; set; }

        /// <summary>
        /// Gets or sets a mapping of the achievement id and whether the user has earned the achievement.
        /// </summary>
        [JsonProperty(PropertyName = "achievements", Required = Required.Always)]
        public IDictionary<int, bool> AchievementsData { get; set; }

        /// <summary>
        /// Gets or sets a mapping of the upgrade id and whether the user has earned the upgrade.
        /// </summary>
        [JsonProperty(PropertyName = "upgrades", Required = Required.Always)]
        public IDictionary<int, bool> UpgradeData { get; set; }

        /// <summary>
        /// Gets or sets the aggregate dps multiplier.
        /// </summary>
        [JsonProperty(PropertyName = "allDpsMultiplier", Required = Required.Always)]
        public double AllDpsMultiplier { get; set; }

        /// <summary>
        /// Gets or sets current number of souls.
        /// </summary>
        [JsonProperty(PropertyName = "heroSouls", Required = Required.Always)]
        public double HeroSouls { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user has purchased the double damage perk with rubies.
        /// </summary>
        [JsonProperty(PropertyName = "paidForRubyMultiplier", Required = Required.Always)]
        public bool HasRubyMultiplier { get; set; }

        /// <summary>
        /// Gets or sets the user's titan damage.
        /// </summary>
        [JsonProperty(PropertyName = "titanDamage", Required = Required.Always)]
        public long TitanDamage { get; set; }

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

            // Remove every other character, AKA "unsprinkle"
            var unsprinkledChars = new char[antiCheatCodeIndex / 2];
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