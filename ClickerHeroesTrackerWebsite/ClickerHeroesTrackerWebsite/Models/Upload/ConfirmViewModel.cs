namespace ClickerHeroesTrackerWebsite.Models.Upload
{
    using Game;
    using SaveData;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;
    using System.Security.Cryptography;
    using System.Text;

    public class ConfirmViewModel
    {
        private static DataContractJsonSerializer serializer = new DataContractJsonSerializer(
            typeof(SavedGame),
            new DataContractJsonSerializerSettings { UseSimpleDictionaryFormat = true });

        public ConfirmViewModel(string encodedSaveData)
        {
            // Decode the save
            var jsonData = DecodeSaveData(encodedSaveData);
            if (jsonData == null)
            {
                return;
            }

            // Deserialize the save
            var savedGame = DeserializeSavedGame(jsonData);
            if (savedGame == null)
            {
                return;
            }

            // Finally, populate the view models
            this.AncientLevelSummaryViewModel = new AncientLevelSummaryViewModel(savedGame.AncientsData);
            this.SuggestedAncientLevelsViewModel = new SuggestedAncientLevelsViewModel(savedGame.AncientsData);
        }

        public AncientLevelSummaryViewModel AncientLevelSummaryViewModel { get; private set; }

        public SuggestedAncientLevelsViewModel SuggestedAncientLevelsViewModel { get; private set; }

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
                // Need to try/catch since this is still based on user input
                try
                {
                    return (SavedGame)serializer.ReadObject(stream);
                }
                catch(SerializationException)
                {
                    return null;
                }
            }
        }
    }
}