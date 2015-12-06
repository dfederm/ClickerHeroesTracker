// <copyright file="SaveData.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.SaveData
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using Newtonsoft.Json;

    [JsonObject]
    public class SavedGame
    {
        private static readonly JsonSerializer serializer = CreateSerializer();

        [JsonProperty(PropertyName = "ancients", Required = Required.Always)]
        public AncientsData AncientsData { get; set; }

        [JsonProperty(PropertyName = "heroCollection", Required = Required.Always)]
        public HeroesData HeroesData { get; set; }

        [JsonProperty(PropertyName = "items", Required = Required.Always)]
        public ItemsData ItemsData { get; set; }

        [JsonProperty(PropertyName = "achievements", Required = Required.Always)]
        public IDictionary<int, bool> AchievementsData { get; set; }

        [JsonProperty(PropertyName = "upgrades", Required = Required.Always)]
        public IDictionary<int, bool> UpgradeData { get; set; }

        [JsonProperty(PropertyName = "allDpsMultiplier", Required = Required.Always)]
        public double AllDpsMultiplier { get; set; }

        [JsonProperty(PropertyName = "heroSouls", Required = Required.Always)]
        public double HeroSouls { get; set; }

        [JsonProperty(PropertyName = "paidForRubyMultiplier", Required = Required.Always)]
        public bool HasRubyMultiplier { get; set; }

        [JsonProperty(PropertyName = "titanDamage", Required = Required.Always)]
        public long TitanDamage { get; set; }

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

        internal static byte[] DecodeSaveData(string encodedSaveData)
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

            // Decode and return
            return Convert.FromBase64CharArray(unsprinkledChars, 0, unsprinkledChars.Length);
        }

        internal static SavedGame DeserializeSavedGame(byte[] saveData)
        {
            using (var stream = new MemoryStream(saveData))
            {
                using (var reader = new StreamReader(stream))
                {
                    return serializer.Deserialize<SavedGame>(new JsonTextReader(reader));
                }
            }
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